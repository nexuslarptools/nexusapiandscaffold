using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Auth0Options _auth0;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory httpClientFactory, IOptions<Auth0Options> auth0Options, ILogger<AuthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _auth0 = auth0Options.Value;
            _logger = logger;
        }

        public class CodeExchangeRequest
        {
            public string code { get; set; } = string.Empty;
            public string redirectUri { get; set; } = string.Empty;
            public string? codeVerifier { get; set; }
            // Optional: override audience used when exchanging code
            public string? audience { get; set; }
        }

        public class CodeExchangeResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string? refresh_token { get; set; }
            public string? id_token { get; set; }
            public string token_type { get; set; } = "Bearer";
            public int expires_in { get; set; }
            public object? user { get; set; }
        }

        private sealed class Auth0TokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string token_type { get; set; } = string.Empty;
            public int expires_in { get; set; }
            public string? refresh_token { get; set; }
            public string? id_token { get; set; }
            public string? scope { get; set; }
        }

        /// <summary>
        /// Exchanges an OAuth2/OIDC authorization code for tokens and returns user info.
        /// </summary>
        /// <remarks>
        /// Body example:
        /// {
        ///   "code": "AUTHORIZATION_CODE",
        ///   "redirectUri": "https://yourapp/callback",
        ///   "codeVerifier": "optional_pkce_verifier"
        /// }
        /// </remarks>
        [HttpPost("ExchangeCode")]
        [AllowAnonymous]
        public async Task<IActionResult> ExchangeCode([FromBody] CodeExchangeRequest request)
        {
            var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier;
            _logger.LogInformation("ExchangeCode called. redirectUri={RedirectUri} code_present={CodePresent} code_verifier_present={CodeVerifierPresent} audience_present={AudiencePresent} TraceId={TraceId}",
                request?.redirectUri,
                !string.IsNullOrWhiteSpace(request?.code),
                !string.IsNullOrWhiteSpace(request?.codeVerifier),
                !string.IsNullOrWhiteSpace(request?.audience),
                traceId);

            if (request == null || string.IsNullOrWhiteSpace(request.code) || string.IsNullOrWhiteSpace(request.redirectUri))
            {
                _logger.LogWarning("ExchangeCode invalid request. redirectUri_present={RedirectUriPresent} code_present={CodePresent} TraceId={TraceId}",
                    !string.IsNullOrWhiteSpace(request?.redirectUri), !string.IsNullOrWhiteSpace(request?.code), traceId);
                return BadRequest(new { error = "invalid_request", error_description = "code and redirectUri are required" });
            }

            if (string.IsNullOrWhiteSpace(_auth0.Domain) || string.IsNullOrWhiteSpace(_auth0.ClientId) || string.IsNullOrWhiteSpace(_auth0.ClientSecret))
            {
                _logger.LogError("Auth0 configuration missing. DomainSet={DomainSet} ClientIdSet={ClientIdSet} ClientSecretSet={ClientSecretSet} TraceId={TraceId}",
                    !string.IsNullOrWhiteSpace(_auth0.Domain), !string.IsNullOrWhiteSpace(_auth0.ClientId), !string.IsNullOrWhiteSpace(_auth0.ClientSecret), traceId);
                return StatusCode(500, new { error = "server_configuration_error", error_description = "Auth0 Domain, ClientId, and ClientSecret must be configured." });
            }

            try
            {
                var issuer = $"https://{_auth0.Domain.Trim().TrimEnd('/')}/";
                var http = _httpClientFactory.CreateClient();
                _logger.LogInformation("Auth0 token request prepared. Issuer={Issuer} AudienceOverride={AudienceOverride} TraceId={TraceId}",
                    issuer, string.IsNullOrWhiteSpace(request.audience) ? null : "provided", traceId);

                // Prepare the token request
                var form = new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["client_id"] = _auth0.ClientId!,
                    ["client_secret"] = _auth0.ClientSecret!,
                    ["code"] = request.code,
                    ["redirect_uri"] = request.redirectUri
                };
                if (!string.IsNullOrWhiteSpace(request.codeVerifier))
                {
                    form["code_verifier"] = request.codeVerifier!;
                }
                if (!string.IsNullOrWhiteSpace(request.audience))
                {
                    form["audience"] = request.audience!;
                }

                using var content = new FormUrlEncodedContent(form);
                using var tokenResp = await http.PostAsync(new Uri(new Uri(issuer), "oauth/token"), content);
                var tokenPayload = await tokenResp.Content.ReadAsStringAsync();
                if (!tokenResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Auth0 token exchange failed: {Status} {Body}", (int)tokenResp.StatusCode, tokenPayload);
                    return BadRequest(new { error = "token_exchange_failed", error_description = tokenPayload });
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var tokens = JsonSerializer.Deserialize<Auth0TokenResponse>(tokenPayload, options);
                if (tokens == null || string.IsNullOrWhiteSpace(tokens.access_token))
                {
                    return BadRequest(new { error = "invalid_token_response", error_description = tokenPayload });
                }

                _logger.LogInformation("Auth0 token exchange succeeded. token_type={TokenType} expires_in={ExpiresIn} has_refresh={HasRefresh} has_id_token={HasId} TraceId={TraceId}",
                    tokens.token_type, tokens.expires_in, !string.IsNullOrWhiteSpace(tokens.refresh_token), !string.IsNullOrWhiteSpace(tokens.id_token), traceId);

                object? user = null;
                try
                {
                    using var userReq = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(issuer), "userinfo"));
                    userReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.access_token);
                    using var userResp = await http.SendAsync(userReq);
                    var userBody = await userResp.Content.ReadAsStringAsync();
                    if (userResp.IsSuccessStatusCode)
                    {
                        user = JsonSerializer.Deserialize<object>(userBody, options);
                        _logger.LogInformation("Auth0 userinfo succeeded: {Status} TraceId={TraceId}", (int)userResp.StatusCode, traceId);
                    }
                    else
                    {
                        _logger.LogWarning("Auth0 userinfo failed: {Status} {Body} TraceId={TraceId}", (int)userResp.StatusCode, userBody, traceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch userinfo from Auth0");
                }

                var response = new CodeExchangeResponse
                {
                    access_token = tokens.access_token,
                    refresh_token = tokens.refresh_token,
                    id_token = tokens.id_token,
                    token_type = string.IsNullOrWhiteSpace(tokens.token_type) ? "Bearer" : tokens.token_type,
                    expires_in = tokens.expires_in,
                    user = user
                };

                _logger.LogInformation("ExchangeCode succeeded. Returning tokens (no secrets). expires_in={ExpiresIn} has_refresh={HasRefresh} has_id_token={HasId} TraceId={TraceId}",
                    response.expires_in, !string.IsNullOrWhiteSpace(response.refresh_token), !string.IsNullOrWhiteSpace(response.id_token), traceId);
                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during code exchange");
                return StatusCode(502, new { error = "upstream_http_error", error_description = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "HTTP timeout during code exchange");
                return StatusCode(504, new { error = "upstream_timeout", error_description = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during code exchange");
                return StatusCode(500, new { error = "server_error", error_description = ex.Message });
            }
        }

        public class LogoutRequest
        {
            public string? returnTo { get; set; }
            public string? clientId { get; set; }
            public string? refreshToken { get; set; }
        }

        public class LogoutResponse
        {
            public string logoutUrl { get; set; } = string.Empty;
            public bool revoked { get; set; }
            public string? revoke_error { get; set; }
        }

        /// <summary>
        /// Generates an Auth0 logout URL and optionally revokes a refresh token.
        /// </summary>
        /// <remarks>
        /// Body example:
        /// {
        ///   "returnTo": "https://yourapp/home",
        ///   "clientId": "optional_override",
        ///   "refreshToken": "optional_refresh_token_to_revoke"
        /// }
        /// </remarks>
        [HttpPost("Logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier;
            _logger.LogInformation("Logout called. returnTo={ReturnTo} clientId_provided={ClientIdProvided} refreshToken_present={RefreshPresent} TraceId={TraceId}",
                request?.returnTo,
                !string.IsNullOrWhiteSpace(request?.clientId),
                !string.IsNullOrWhiteSpace(request?.refreshToken),
                traceId);
            try
            {
                var domain = _auth0.Domain?.Trim().TrimEnd('/') ?? string.Empty;
                var clientId = !string.IsNullOrWhiteSpace(request?.clientId) ? request!.clientId! : _auth0.ClientId;
                if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(clientId))
                {
                    _logger.LogError("Logout configuration error. DomainSet={DomainSet} ClientIdSet={ClientIdSet} TraceId={TraceId}",
                        !string.IsNullOrWhiteSpace(domain), !string.IsNullOrWhiteSpace(clientId), traceId);
                    return StatusCode(500, new { error = "server_configuration_error", error_description = "Auth0 Domain and ClientId must be configured." });
                }

                var issuer = $"https://{domain}/";

                // Build logout URL
                var logoutBase = new Uri(new Uri(issuer), "v2/logout");
                var query = $"client_id={Uri.EscapeDataString(clientId!)}";
                if (!string.IsNullOrWhiteSpace(request?.returnTo))
                {
                    query += $"&returnTo={Uri.EscapeDataString(request!.returnTo!)}";
                }
                var logoutUriBuilder = new UriBuilder(logoutBase) { Query = query };

                var response = new LogoutResponse { logoutUrl = logoutUriBuilder.Uri.ToString(), revoked = false };

                _logger.LogInformation("Built logout URL. Issuer={Issuer} LogoutUrl={LogoutUrl} TraceId={TraceId}", issuer, response.logoutUrl, traceId);

                // Optionally revoke refresh token
                if (!string.IsNullOrWhiteSpace(request?.refreshToken))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(_auth0.ClientSecret))
                        {
                            // Cannot revoke without client secret
                            response.revoke_error = "client_secret_not_configured";
                        }
                        else
                        {
                            var http = _httpClientFactory.CreateClient();
                            var form = new Dictionary<string, string>
                            {
                                ["client_id"] = clientId!,
                                ["client_secret"] = _auth0.ClientSecret!,
                                ["token"] = request!.refreshToken!,
                                ["token_type_hint"] = "refresh_token"
                            };
                            using var content = new FormUrlEncodedContent(form);
                            using var revokeResp = await http.PostAsync(new Uri(new Uri(issuer), "oauth/revoke"), content);
                            var body = await revokeResp.Content.ReadAsStringAsync();
                            if (revokeResp.IsSuccessStatusCode)
                            {
                                response.revoked = true;
                                _logger.LogInformation("Refresh token revoked successfully. TraceId={TraceId}", traceId);
                            }
                            else
                            {
                                _logger.LogWarning("Auth0 revoke failed: {Status} {Body} TraceId={TraceId}", (int)revokeResp.StatusCode, body, traceId);
                                response.revoke_error = body;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to revoke refresh token");
                        response.revoke_error = ex.Message;
                    }
                }

                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during logout");
                return StatusCode(502, new { error = "upstream_http_error", error_description = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "HTTP timeout during logout");
                return StatusCode(504, new { error = "upstream_timeout", error_description = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout");
                return StatusCode(500, new { error = "server_error", error_description = ex.Message });
            }
        }
    }
}
