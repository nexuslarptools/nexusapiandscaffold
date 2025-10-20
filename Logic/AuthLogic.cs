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

            _client = new ManagementApiClient(_managementBearer.Token, new Uri("https://dev-3xazewbu.auth0.com/api/v2"));
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
            var _httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://dev-3xazewbu.auth0.com/oauth/token");
            var contentHeader = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName };
            var content = new ManagementTokenRequestContent()
            {
                client_id = "DOctic0k94km5UB0Mnvxduk6wuvZUZ9q",
                client_secret = "5nH9ypMlFrZvXmr__Hgm85yoFI9yRUm4yC_ssu3_kgYaT2E443XIjeunktWS5pF5",
                audience = "https://dev-3xazewbu.auth0.com/api/v2/",
                grant_type = "client_credentials"
            };

            request.Content = JsonContent.Create(content, contentHeader);
            var response = await _httpClient.SendAsync(request);
            var token = await response.Content.ReadFromJsonAsync<ManagementTokenResponse>();
            var managementToken = new ManagementToken()
            {
                Token = token.Token,
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
