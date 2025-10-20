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
        /// Deprecated: Frontend and proxy middleware now perform the authorization code exchange.
        /// This endpoint no longer exchanges codes server-side.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("ExchangeCode")]
        [Consumes("application/x-www-form-urlencoded", "application/json")]
        [Produces("application/json")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ExchangeCode()
        {
            _logger.LogInformation("ExchangeCode called, but token exchange is now handled by fronting middleware. Returning 410.");
            return StatusCode(StatusCodes.Status410Gone, new { message = "exchange_handled_by_middleware", detail = "This API no longer performs the token exchange. The reverse proxy/BFF middleware completes the OIDC flow and forwards authenticated requests." });
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
