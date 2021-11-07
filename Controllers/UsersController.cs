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
using NEXUSDataLayerScaffold.Logic;
using Microsoft.AspNetCore.Authentication;
using System.Web;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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
            AuthUser result = await UsersLogic.GetUserInfo(accessToken, _context);


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
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            if (result.Result.roles.Any(l => l.Contains("GM")) || result.Result.roles.Any(l => l.Contains("Wizard")))
            {
                var UsersList = _context.Users.Where(u => u.Guid == id).ToList();
                return Ok(UsersList);
            }
            return Ok("Not Authorized");
        }
    }
}