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
using Microsoft.EntityFrameworkCore;

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


        // PUT: api/v1/users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> PutUser(Guid guid, Users user)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                if (guid != user.Guid)
                {
                    return BadRequest();
                }

                var oldUserinfo = _context.Users.Where(u => u.Guid == user.Guid).FirstOrDefault();

                if (oldUserinfo == null)
                {
                    return NotFound();
                }

                if (oldUserinfo.Firstname != user.Firstname && user.Firstname != null)
                {
                    oldUserinfo.Firstname = user.Firstname;
                }

                if (oldUserinfo.Lastname != user.Lastname && user.Lastname != null)
                {
                    oldUserinfo.Firstname = user.Firstname;
                }

                if (oldUserinfo.Preferredname != user.Preferredname && user.Preferredname != null)
                {
                    oldUserinfo.Preferredname = user.Preferredname;
                }

                _context.Users.Update(oldUserinfo);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;

                }

                return NoContent();
            }
            return Unauthorized();
        }
    }
}