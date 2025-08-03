using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Amazon.RuntimeDependencies.SecurityTokenServiceClientContext;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ShipCrewListController : ControllerBase
    {
        private readonly NexusLarpLocalContext _context;

        public ShipCrewListController(NexusLarpLocalContext context)
        {
            _context = context;
        }

        // Get
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<CrewRolesDO>> GetShipCrewList()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var output = new CrewRolesDO();

                output.Roles = _context.ShipCrewLists.Where(scl => scl.Isactive == true)
                    .OrderBy(scl => scl.Ord)
                    .Select(scl => new CrewRole(scl)).ToList();

                var defaultRole = new CrewRole();
                defaultRole.Default();

                output.Roles.Add(defaultRole);

                return Ok(output);
            }

            return Unauthorized();
        }


        // Post WIZARDS ONLY
        // Get
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CrewRole>> AddShipCrew([FromBody] CrewRole input)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                try
                {
                    _context.ShipCrewLists.Add(input.ToShipCrewList());
                    _context.SaveChanges();
                    return CreatedAtAction("PostShipCrewList", new { id = input.Guid }, input);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }

            return Unauthorized();
        }

        // Put WIZARDS ONLY
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> PutCrewPosition(Guid guid, CrewRole input)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                if (input.Guid != guid) 
                {
                    return BadRequest("Mismatched identifiers");
                }

                try
                {
                    var changeRole = _context.ShipCrewLists.Where(scl => scl.Guid == guid).FirstOrDefault();

                    changeRole.Ord = input.ord;
                    changeRole.Position = input.Position;
                    changeRole.Details = input.Description;

                    _context.SaveChanges();

                    return Ok(input);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

            }
            return Unauthorized();
        }

        // Put WIZARDS ONLY
        [HttpPut("disable/{guid}")]
        [Authorize]
        public async Task<IActionResult> DeactivateCrewPosition(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                try
                {
                    var changeRole = _context.ShipCrewLists.Where(scl => scl.Guid == guid).FirstOrDefault();

                    changeRole.Isactive = false;
                    _context.SaveChanges();

                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

            }
            return Unauthorized();
        }

        // Put WIZARDS ONLY
        [HttpPut("enable/{guid}")]
        [Authorize]
        public async Task<IActionResult> ReactivateCrewPosition(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                try
                {
                    var changeRole = _context.ShipCrewLists.Where(scl => scl.Guid == guid).FirstOrDefault();

                    changeRole.Isactive = true;
                    _context.SaveChanges();

                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

            }
            return Unauthorized();
        }

        // Delete WIZARDS ONLY
        // Put WIZARDS ONLY
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<IActionResult> FullDeleteCrewPosition(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                try
                {
                    var changeRole = _context.ShipCrewLists.Where(scl => scl.Guid == guid).FirstOrDefault();
                    _context.ShipCrewLists.Remove(changeRole);
                    _context.SaveChanges();

                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

            }
            return Unauthorized();
        }

    }
}
