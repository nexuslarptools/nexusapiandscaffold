using Auth0.ManagementApi;
using Azure.Core;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Collections.Generic;
using Auth0.ManagementApi.Models;
using System.Net;
using NEXUSDataLayerScaffold.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace NEXUSDataLayerScaffold.Logic
{
    public class AuthLogic
    {
        private ManagementToken _managementBearer = new ManagementToken();
        private ManagementApiClient _client;

        // Static, process-wide cache for the Auth0 Management API token to avoid frequent token requests
        private static readonly object _tokenLock = new object();
        private static ManagementToken? _cachedToken;

        private static string? GetConfigValue(string key)
        {
            try
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{env}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                return config[key];
            }
            catch
            {
                return null;
            }
        }

        public AuthLogic() {
            // Load token from store. check expy, if it's not fresh, grab new token.
            _managementBearer = GetToken().GetAwaiter().GetResult();

            var domain = GetConfigValue("Auth0:Domain") ?? Environment.GetEnvironmentVariable("Auth0__Domain");
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new InvalidOperationException("Auth0 domain is not configured. Provide Auth0:Domain or Auth0__Domain.");
            }

            _client = new ManagementApiClient(_managementBearer.Token, new Uri($"https://{domain.Trim().TrimEnd('/')}/api/v2"));
        }

        public async Task<IList<User>> GetAllUsersByEmail(string email)
        {
            var results = await _client.Users.GetUsersByEmailAsync(email);
            return results;
        }

        public async Task<User> GetUserByAuthID(string auth)
        {
            var results = await _client.Users.GetAsync(auth);
            return results;
        }

        public async Task<User> UpdateUserRoles(string auth, MetadataRoles mdata)
        {
            try
            {
                UserUpdateRequest updateRequest = new UserUpdateRequest();
                updateRequest.AppMetadata = mdata;
                var results = await _client.Users.UpdateAsync(auth, updateRequest);
                return results;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        private async Task<ManagementToken> GetToken()
        {
            // Serve from cache if valid for at least 60 more seconds
            lock (_tokenLock)
            {
                if (_cachedToken != null && _cachedToken.ExpirationTime > DateTime.UtcNow.AddSeconds(60))
                {
                    return _cachedToken;
                }
            }

            var domain = GetConfigValue("Auth0:Domain") ?? Environment.GetEnvironmentVariable("Auth0__Domain");
            var clientId = GetConfigValue("Auth0:ClientId") ?? Environment.GetEnvironmentVariable("Auth0__ClientId");
            var clientSecret = GetConfigValue("Auth0:ClientSecret") ?? Environment.GetEnvironmentVariable("Auth0__ClientSecret");

            if (string.IsNullOrWhiteSpace(domain))
                throw new InvalidOperationException("Auth0:Domain/Auth0__Domain is required for Management API token.");
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("Auth0:ClientId/Auth0__ClientId and Auth0:ClientSecret/Auth0__ClientSecret are required for Management API token.");

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{domain}/oauth/token");

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["audience"] = $"https://{domain}/api/v2/"
            };

            request.Content = new FormUrlEncodedContent(form);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("Failed to obtain Auth0 management token (403 Forbidden). Ensure the configured ClientId/ClientSecret belong to a Machine-to-Machine application that is authorized to the Auth0 Management API with the correct audience (https://{domain}/api/v2/). No sensitive details were logged.");
                }
                // Intentionally avoid logging sensitive data
                throw new InvalidOperationException($"Failed to obtain Auth0 management token. Status {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            var token = await response.Content.ReadFromJsonAsync<ManagementTokenResponse>();
            if (token == null || string.IsNullOrWhiteSpace(token.Token))
                throw new InvalidOperationException("Auth0 management token response was empty.");

            var expiresIn = token.ExpiresIn > 0 ? token.ExpiresIn : 24 * 60 * 60;
            var managementToken = new ManagementToken
            {
                Token = token.Token,
                ExpirationTime = DateTime.UtcNow.AddSeconds(Math.Max(0, expiresIn - 60))
            };

            // Update cache atomically
            lock (_tokenLock)
            {
                _cachedToken = managementToken;
                return _cachedToken;
            }
        }

        public class ManagementTokenRequestContent
        {
            public string client_id { get; set; } = string.Empty;

            public string client_secret { get; set; } = string.Empty;

            public string audience { get; set; } = string.Empty;

            public string grant_type { get; set; } = string.Empty;
        }

        public class ManagementToken
        {
            public string Token { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

        public class ManagementTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

    }
}
