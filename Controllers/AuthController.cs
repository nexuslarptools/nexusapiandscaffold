using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Auth0Options _auth0;
        private readonly FrontendOptions _frontend;
        private readonly SecurityOptions _security;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory httpClientFactory,
                              IOptions<Auth0Options> auth0,
                              IOptions<FrontendOptions> frontend,
                              IOptions<SecurityOptions> security,
                              ILogger<AuthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _auth0 = auth0.Value;
            _frontend = frontend.Value ?? new FrontendOptions();
            _security = security.Value ?? new SecurityOptions();
            _logger = logger;
        }

        /// <summary>
        /// Backchannel token exchange for OIDC Authorization Code Flow (supports PKCE). Establishes cookie session.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("ExchangeCode")]
        [Consumes("application/x-www-form-urlencoded", "application/json")]
        [Produces("application/json")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ExchangeCode()
        {
            string? q(string key)
            {
                return Request.Query.TryGetValue(key, out var v) ? v.ToString() : null;
            }

            string? code = q("code");
            string? state = q("state");
            string? codeVerifier = q("code_verifier");
            string? redirectUri = q("redirect_uri");
            string? audience = q("audience");
            string? returnUrl = q("returnUrl");

            // Parse form if present
            if (string.Equals(Request.ContentType, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
                || (Request.ContentType?.StartsWith("application/x-www-form-urlencoded") ?? false))
            {
                var form = await Request.ReadFormAsync();
                code ??= form["code"].FirstOrDefault();
                state ??= form["state"].FirstOrDefault();
                codeVerifier ??= form["code_verifier"].FirstOrDefault();
                redirectUri ??= form["redirect_uri"].FirstOrDefault();
                audience ??= form["audience"].FirstOrDefault();
                returnUrl ??= form["returnUrl"].FirstOrDefault();
            }
            else if ((Request.ContentType?.StartsWith("application/json") ?? false))
            {
                using var reader = new System.IO.StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        code ??= root.TryGetProperty("code", out var p1) ? p1.GetString() : null;
                        state ??= root.TryGetProperty("state", out var p2) ? p2.GetString() : null;
                        codeVerifier ??= root.TryGetProperty("code_verifier", out var p3) ? p3.GetString() : null;
                        redirectUri ??= root.TryGetProperty("redirect_uri", out var p4) ? p4.GetString() : null;
                        audience ??= root.TryGetProperty("audience", out var p5) ? p5.GetString() : null;
                        returnUrl ??= root.TryGetProperty("returnUrl", out var p6) ? p6.GetString() : null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "ExchangeCode: Failed to parse JSON body");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { error = "invalid_request", error_description = "Missing code" });
            }

            var resolvedAudience = !string.IsNullOrWhiteSpace(audience) ? audience : _auth0.ApiIdentifier;

            var resolvedRedirectUri = !string.IsNullOrWhiteSpace(redirectUri)
                ? redirectUri
                : (!string.IsNullOrWhiteSpace(_auth0.RedirectUri) ? _auth0.RedirectUri : InferCallbackFromRequest());

            var tokenEndpoint = $"https://{_auth0.Domain}/oauth/token";
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var kv = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "authorization_code"),
                new("code", code),
                new("redirect_uri", resolvedRedirectUri)
            };

            if (!string.IsNullOrWhiteSpace(_auth0.ClientId))
                kv.Add(new KeyValuePair<string, string>("client_id", _auth0.ClientId!));

            if (!string.IsNullOrWhiteSpace(_auth0.ClientSecret))
            {
                if (string.Equals(_auth0.ClientAuthMethod, "client_secret_basic", StringComparison.OrdinalIgnoreCase))
                {
                    var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_auth0.ClientId}:{_auth0.ClientSecret}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
                }
                else
                {
                    kv.Add(new KeyValuePair<string, string>("client_secret", _auth0.ClientSecret!));
                }
            }

            if (!string.IsNullOrWhiteSpace(resolvedAudience))
                kv.Add(new KeyValuePair<string, string>("audience", resolvedAudience));

            if (!string.IsNullOrWhiteSpace(codeVerifier))
                kv.Add(new KeyValuePair<string, string>("code_verifier", codeVerifier));

            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(kv));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExchangeCode: token endpoint unreachable");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "token_endpoint_unreachable" });
            }

            var text = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                // Proxy error when possible
                try
                {
                    return StatusCode((int)resp.StatusCode, JsonDocument.Parse(text).RootElement);
                }
                catch
                {
                    return StatusCode((int)resp.StatusCode, new { error = "token_exchange_failed", state });
                }
            }

            string? accessToken = null, idToken = null, refreshToken = null;
            int? expiresIn = null;
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                idToken = root.TryGetProperty("id_token", out var it) ? it.GetString() : null;
                refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
                expiresIn = root.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ExchangeCode: token response parse issue");
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            // Best-effort: extract sub/name/email from id_token if present
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                try
                {
                    var claims = ParseJwtClaims(idToken!);
                    identity.AddClaims(claims);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ExchangeCode: failed to parse id_token claims");
                }
            }

            if (!identity.Claims.Any())
            {
                // Minimal principal to mark user as authenticated
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
            }

            var props = new AuthenticationProperties();
            var tokens = new List<AuthenticationToken>();
            if (!string.IsNullOrWhiteSpace(accessToken)) tokens.Add(new AuthenticationToken { Name = "access_token", Value = accessToken });
            if (!string.IsNullOrWhiteSpace(idToken)) tokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken });
            if (!string.IsNullOrWhiteSpace(refreshToken)) tokens.Add(new AuthenticationToken { Name = "refresh_token", Value = refreshToken });
            if (expiresIn.HasValue)
            {
                var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn.Value);
                tokens.Add(new AuthenticationToken { Name = "expires_at", Value = expiresAt.ToString("o") });
            }
            props.StoreTokens(tokens);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

            // Decide redirect
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                if (IsAllowedReturnUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            if (!string.IsNullOrWhiteSpace(_frontend?.DefaultReturnUrl) && IsAllowedReturnUrl(_frontend.DefaultReturnUrl))
            {
                return Redirect(_frontend.DefaultReturnUrl);
            }

            return Ok(new { message = "token_exchange_success", state, sessionEstablished = true });
        }

        private string InferCallbackFromRequest()
        {
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
            var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? Request.Host.Value;
            return $"{scheme}://{host}/oauth2/callback";
        }

        private bool IsAllowedReturnUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (Uri.TryCreate(url, UriKind.Relative, out _)) return true; // allow relative paths
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            var host = uri.Host;
            var allowed = _security.AllowedRedirectHosts ?? Array.Empty<string>();
            return allowed.Contains(host, StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<Claim> ParseJwtClaims(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) yield break;
            string Base64UrlDecode(string s)
            {
                s = s.Replace('-', '+').Replace('_', '/');
                switch (s.Length % 4)
                {
                    case 2: s += "=="; break;
                    case 3: s += "="; break;
                }
                var bytes = Convert.FromBase64String(s);
                return Encoding.UTF8.GetString(bytes);
            }

            var payloadJson = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("sub", out var sub))
                yield return new Claim(ClaimTypes.NameIdentifier, sub.GetString() ?? string.Empty);
            if (root.TryGetProperty("name", out var name))
                yield return new Claim(ClaimTypes.Name, name.GetString() ?? string.Empty);
            if (root.TryGetProperty("email", out var email))
                yield return new Claim(ClaimTypes.Email, email.GetString() ?? string.Empty);
        }
    }
}
