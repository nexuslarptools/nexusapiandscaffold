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
            if (request == null || string.IsNullOrWhiteSpace(request.code) || string.IsNullOrWhiteSpace(request.redirectUri))
            {
                return BadRequest(new { error = "invalid_request", error_description = "code and redirectUri are required" });
            }

            if (string.IsNullOrWhiteSpace(_auth0.Domain) || string.IsNullOrWhiteSpace(_auth0.ClientId) || string.IsNullOrWhiteSpace(_auth0.ClientSecret))
            {
                return StatusCode(500, new { error = "server_configuration_error", error_description = "Auth0 Domain, ClientId, and ClientSecret must be configured." });
            }

            try
            {
                var issuer = $"https://{_auth0.Domain.Trim().TrimEnd('/')}/";
                var http = _httpClientFactory.CreateClient();

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
                    }
                    else
                    {
                        _logger.LogWarning("Auth0 userinfo failed: {Status} {Body}", (int)userResp.StatusCode, userBody);
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
    }
}
