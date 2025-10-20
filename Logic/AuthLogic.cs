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

namespace NEXUSDataLayerScaffold.Logic
{
    public class AuthLogic
    {
        private ManagementToken _managementBearer = new ManagementToken();
        private ManagementApiClient _client;

        public AuthLogic() {
            // Load token from store. check expy, if it's not fresh, grab new token.
            _managementBearer = GetToken().Result;

            var domain = GetEnvironmentValue(
                new[] { "Auth0__Domain", "AUTH0_DOMAIN" },
                required: true);
            var managementBase = $"https://{domain}/api/v2";
            _client = new ManagementApiClient(_managementBearer.Token, new Uri(managementBase));
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
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetEnvironmentValue(string[] keys, bool required = false, string? defaultValue = null)
        {
            foreach (var key in keys)
            {
                var v = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrWhiteSpace(v))
                {
                    return v;
                }
            }
            if (defaultValue != null) return defaultValue;
            if (required)
            {
                throw new InvalidOperationException($"Missing required environment variable. Tried: {string.Join(", ", keys)}");
            }
            return string.Empty;
        }

        private async Task<ManagementToken> GetToken()
        {
            var domain = GetEnvironmentValue(new[] { "Auth0__Domain", "AUTH0_DOMAIN" }, required: true);
            var tokenUrl = $"https://{domain}/oauth/token";
            var audience = GetEnvironmentValue(new[] { "Auth0__ManagementAudience", "AUTH0_MANAGEMENT_AUDIENCE" }, defaultValue: $"https://{domain}/api/v2/");
            var clientId = GetEnvironmentValue(new[] { "Auth0__ManagementClientId", "AUTH0_MANAGEMENT_CLIENT_ID", "Auth0__ClientId", "AUTH0_CLIENT_ID" }, required: true);
            var clientSecret = GetEnvironmentValue(new[] { "Auth0__ManagementClientSecret", "AUTH0_MANAGEMENT_CLIENT_SECRET", "Auth0__ClientSecret", "AUTH0_CLIENT_SECRET" }, required: true);

            var _httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            var contentHeader = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName };
            var content = new ManagementTokenRequestContent()
            {
                client_id = clientId,
                client_secret = clientSecret,
                audience = audience,
                grant_type = "client_credentials"
            };

            request.Content = JsonContent.Create(content, contentHeader);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<ManagementTokenResponse>();
            var managementToken = new ManagementToken()
            {
                Token = token?.Token ?? string.Empty,
                ExpirationTime = DateTime.UtcNow.AddHours(23),
            };
            return managementToken;
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
        }

    }
}
