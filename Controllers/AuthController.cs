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
        private readonly OAuthOptions? _oauth;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory httpClientFactory,
                              IOptions<Auth0Options> auth0,
                              IOptions<FrontendOptions> frontend,
                              IOptions<SecurityOptions> security,
                              IOptions<OAuthOptions>? oauth,
                              ILogger<AuthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _auth0 = auth0.Value;
            _frontend = frontend.Value ?? new FrontendOptions();
            _security = security.Value ?? new SecurityOptions();
            _oauth = oauth?.Value;
            _logger = logger;
        }

        /// <summary>
        /// Exchanges an authorization code for tokens and establishes a cookie session.
        /// Accepts JSON or x-www-form-urlencoded with at least 'code' and optionally 'state', 'redirect_uri', 'code_verifier'.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("ExchangeCode")]
        [Consumes("application/x-www-form-urlencoded", "application/json")]
        [Produces("application/json")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ExchangeCode([FromBody] ExchangeCodeRequest? body)
        {
            // Support both JSON body and form posts
            string? code = body?.code;
            string? state = body?.state;
            string? redirectUri = body?.redirect_uri;
            string? codeVerifier = body?.code_verifier;
            // Optional frontend return URL to redirect after establishing session
            string? returnUrl = Request.Query["returnUrl"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                // accept both camelCase and snake_case keys if sent in JSON body
                returnUrl = (Request.HasFormContentType ? null : HttpContext.Request.Headers["X-Return-Url"].FirstOrDefault())
                           ?? (body as dynamic)?.returnUrl;
            }

            if (string.IsNullOrWhiteSpace(code) && Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                code = form["code"].FirstOrDefault();
                state = form["state"].FirstOrDefault();
                redirectUri = redirectUri ?? form["redirect_uri"].FirstOrDefault();
                codeVerifier = codeVerifier ?? form["code_verifier"].FirstOrDefault();
                returnUrl = returnUrl ?? form["returnUrl"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { error = "invalid_request", error_description = "Missing 'code'" });
            }

            try
            {
                var authority = !string.IsNullOrWhiteSpace(_oauth?.Authority) ? _oauth!.Authority.TrimEnd('/') : $"https://{_auth0.Domain}";
                var tokenEndpoint = $"{authority}/oauth/token"; // generic default works for many IdPs (incl. Auth0)
                var clientId = _auth0.ClientId ?? string.Empty;
                var clientSecret = _auth0.ClientSecret;
                var clientAuthMethod = _auth0.ClientAuthMethod ?? "client_secret_post";

                // Determine redirect URI
                if (string.IsNullOrWhiteSpace(redirectUri))
                {
                    redirectUri = _auth0.RedirectUri ?? InferCallbackFromRequest();
                }

                using var client = _httpClientFactory.CreateClient();

                var formFields = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "authorization_code"),
                    new("code", code),
                    new("redirect_uri", redirectUri),
                    new("client_id", clientId)
                };

                if (!string.IsNullOrWhiteSpace(codeVerifier))
                {
                    formFields.Add(new KeyValuePair<string, string>("code_verifier", codeVerifier));
                }
                else if (!string.IsNullOrWhiteSpace(clientSecret) && string.Equals(clientAuthMethod, "client_secret_post", StringComparison.OrdinalIgnoreCase))
                {
                    formFields.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                }
                else if (!string.IsNullOrWhiteSpace(clientSecret) && string.Equals(clientAuthMethod, "client_secret_basic", StringComparison.OrdinalIgnoreCase))
                {
                    var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
                }

                var resp = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(formFields));
                var payload = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ExchangeCode token endpoint returned {Status}: {Body}", (int)resp.StatusCode, payload);
                    return StatusCode((int)resp.StatusCode, new { error = "token_exchange_failed", detail = payload });
                }

                var token = JsonDocument.Parse(payload).RootElement;
                var accessToken = token.TryGetProperty("access_token", out var at) ? at.GetString() : null;
                var idToken = token.TryGetProperty("id_token", out var it) ? it.GetString() : null;
                var refreshToken = token.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
                var expiresIn = token.TryGetProperty("expires_in", out var ei) && ei.TryGetInt32(out var secs) ? secs : 3600;

                // Build identity from ID token if available, otherwise from access token claims
                IEnumerable<Claim> claims = Array.Empty<Claim>();
                if (!string.IsNullOrWhiteSpace(idToken))
                {
                    claims = ParseJwtClaims(idToken!);
                }
                else if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    claims = ParseJwtClaims(accessToken!);
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
                };
                var storedTokens = new List<AuthenticationToken>();
                if (!string.IsNullOrWhiteSpace(accessToken)) storedTokens.Add(new AuthenticationToken { Name = "access_token", Value = accessToken });
                if (!string.IsNullOrWhiteSpace(idToken)) storedTokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken });
                if (!string.IsNullOrWhiteSpace(refreshToken)) storedTokens.Add(new AuthenticationToken { Name = "refresh_token", Value = refreshToken });
                storedTokens.Add(new AuthenticationToken { Name = "expires_at", Value = DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("o") });
                props.StoreTokens(storedTokens);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                // If a returnUrl is supplied and allowed, redirect to it after saving the session cookie
                if (!string.IsNullOrWhiteSpace(returnUrl) && IsAllowedReturnUrl(returnUrl))
                {
                    // Use 303 See Other to force GET on the frontend URL
                    return new RedirectResult(returnUrl, permanent: false, preserveMethod: false);
                }

                // Return session info
                return await Session();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExchangeCode failed");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", detail = ex.Message });
            }
        }

        public class ExchangeCodeRequest
        {
            public string? code { get; set; }
            public string? state { get; set; }
            public string? redirect_uri { get; set; }
            public string? code_verifier { get; set; }
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

        /// <summary>
        /// Returns basic session information. Useful for checking if the user is logged in.
        /// </summary>
        [HttpGet("Session")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Session()
        {
            // Prefer the default (cookie) principal
            var principal = HttpContext.User;
            var isAuthenticated = principal?.Identity?.IsAuthenticated == true;
            string? authType = principal?.Identity?.AuthenticationType;

            // If not authenticated via cookie, attempt JwtBearer explicitly as a fallback
            if (!isAuthenticated)
            {
                try
                {
                    var jwtResult = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
                    if (jwtResult.Succeeded && jwtResult.Principal != null)
                    {
                        principal = jwtResult.Principal;
                        isAuthenticated = true;
                        authType = jwtResult.Ticket?.AuthenticationScheme ?? "Bearer";
                    }
                }
                catch
                {
                    // ignore
                }
            }

            string? accessToken = null, idToken = null, refreshToken = null, expiresAt = null;
            try
            {
                accessToken = await HttpContext.GetTokenAsync("access_token");
                idToken = await HttpContext.GetTokenAsync("id_token");
                refreshToken = await HttpContext.GetTokenAsync("refresh_token");
                expiresAt = await HttpContext.GetTokenAsync("expires_at");
            }
            catch
            {
                // ignore
            }

            var sub = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal?.FindFirst("sub")?.Value;
            var name = principal?.FindFirst("name")?.Value ?? principal?.Identity?.Name;
            var email = principal?.FindFirst(ClaimTypes.Email)?.Value ?? principal?.FindFirst("email")?.Value;
            var roles = principal?.FindAll("https://NexusLarps.com/roles").Select(c => c.Value).Distinct().ToArray() ?? Array.Empty<string>();

            var result = new
            {
                isAuthenticated,
                authType,
                subject = sub,
                name,
                email,
                roles,
                hasAccessToken = !string.IsNullOrWhiteSpace(accessToken),
                hasIdToken = !string.IsNullOrWhiteSpace(idToken),
                hasRefreshToken = !string.IsNullOrWhiteSpace(refreshToken),
                expiresAt
            };

            return Ok(result);
        }
    }
}
