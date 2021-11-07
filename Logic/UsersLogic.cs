using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Logic
{
    public class UsersLogic
    {
        private readonly NexusLARPContextBase _context;

        public UsersLogic(NexusLARPContextBase context)
        {
            _context = context;
        }

        public static async Task<AuthUser> GetUserInfo(string accessToken2, NexusLARPContextBase _context)
        {
            // Get the access token.

            AuthUser returnuser = new AuthUser();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);

            HttpResponseMessage responseMessage = await client.GetAsync("https://dev-3xazewbu.auth0.com/userinfo");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var JsonHoldingCell = JsonDocument.Parse(responseData);

                returnuser.name = JsonHoldingCell.RootElement.GetProperty("name").ToString();
                returnuser.email = JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/email").ToString();

                List<string> permissionsholder = new List<string>();
                for (int i = 0; i < JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/permissions").GetArrayLength(); i++)
                {
                    permissionsholder.Add(JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/permissions")[i].ToString());
                }
                returnuser.permissions = permissionsholder;

                permissionsholder = new List<string>();
                for (int i = 0; i < JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/roles").GetArrayLength(); i++)
                {
                    permissionsholder.Add(JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/roles")[i].ToString());
                }
                returnuser.roles = permissionsholder;


                // check if the user is in the users table, if not, add them.
                    var checkUser = _context.Users.Where(u => u.Email == returnuser.email).FirstOrDefault();

                    if (checkUser == null)
                    {
                        Users newUsers = new Users
                        {
                            Email = returnuser.email.ToString(),
                            Preferredname = returnuser.name
                        };

                    _context.Users.Add(newUsers);
                    _context.SaveChanges();

                        returnuser.userGuid = _context.Users.Where(u => u.Email == returnuser.email).FirstOrDefault().Guid;

                    }
                    else
                    {
                        returnuser.userGuid = checkUser.Guid;
                    }

                return returnuser;
            }
            return returnuser;

        }
    }
}
