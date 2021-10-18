using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NEXUSDataLayerScaffold.Models;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using NEXUSDataLayerScaffold.Entities;
using Microsoft.AspNetCore.Authentication;
using System.Web;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public UsersController(NexusLARPContextBase context)
        {
            _context = context;
        }


        public static string GetUser(HttpContext httpContext)
        {
            string email = "";

            return email;
        }

        [Authorize]
        public static async Task<AuthUser> GetUserInfo(string accessToken2)
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
                using (var context = new NexusLARPContextBase())
                {
                    var checkUser = context.Users.Where(u => u.Email == returnuser.email).FirstOrDefault();

                    if (checkUser == null)
                    {
                        Users newUsers = new Users {
                            Email = returnuser.email.ToString(),
                            Preferredname = returnuser.name
                        };

                        context.Users.Add(newUsers);
                        context.SaveChanges();

                        returnuser.userGuid = context.Users.Where(u => u.Email == returnuser.email).FirstOrDefault().Guid;

                    }
                    else
                    {
                        returnuser.userGuid = checkUser.Guid;
                    }
                };

                return returnuser;
            }
            return returnuser;

        }

        public static bool UserPermissionAuth(AuthUser user, string authName)
        {
            if (user.permissions != null)
            {
                if (user.permissions.Contains(authName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a list of all users.
        /// </summary>
        /// <returns></returns>
        // GET api/v1/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUsers()
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            AuthUser result = await GetUserInfo(accessToken);


            if (result.roles.Contains("Wizard"))
            {
                var UsersList = _context.Users.ToList();
                return Ok(UsersList);
            }
            return Ok("Not Authorized");
        }

        /// <summary>
        /// Gets a single user's information
        /// </summary>
        /// <returns></returns>
        // GET api/v1/Users/{id}
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<IEnumerable<Users>> GetAUser(Guid id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = GetUserInfo(accessToken);

            if (result.Result.roles.Any(l => l.Contains("GM")) || result.Result.roles.Any(l => l.Contains("Wizard")))
            {
                var UsersList = _context.Users.Where(u => u.Guid == id).ToList();
                return Ok(UsersList);
            }
            return Ok("Not Authorized");
        }
    }
}