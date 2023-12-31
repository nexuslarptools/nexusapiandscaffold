using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public RolesController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Returns all Tag Types and Guids
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/TagTypes
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<RoleOut>>> GetRoles()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            return await _context.Roles.Select(r => new RoleOut(r.Id, r.Rolename)).ToListAsync();

        if (UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
            return await _context.Roles.Where(ro => ro.Rolename != "HeadGM" && ro.Rolename != "Wizard").Select(r => new RoleOut(r.Id, r.Rolename)).ToListAsync();

        return Unauthorized();
    }


    /// <summary>
    ///     Accepts a Tag type's guid and JSON schema in the body to update information.  WIZARD ACCESS ONLY.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tagTypes"></param>
    /// <returns></returns>
    // PUT: api/V1/TagTypes/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutRoles(int id, [FromBody] Roles role)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            if (id != role.Id) return BadRequest();

            _context.Entry(role).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Accepts tagtype schema JSON in the body and creates a new tag type. WIZARD ACCESS ONLY!
    /// </summary>
    /// <param name="tagTypes"></param>
    /// <returns></returns>
    // POST: api/TagTypes
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Role>> PostTagTypes(Role role)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoles", new { guid = role.Id }, role);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Deletes a TagType from the list WIZARD ACCESS ONLY, All related tags must be FULLY deleted first, may cause
    ///     orhpaned guids in sheets!!
    /// </summary>
    /// <param name="tagTypes"></param>
    /// <returns></returns>
    // DELETE: api/TagTypes/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<Role>> DeleteRole(int id)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var roles = await _context.Roles.FindAsync(id);
            if (roles == null) return NotFound();

            _context.Roles.Remove(roles);
            await _context.SaveChangesAsync();

            return roles;
        }

        return Unauthorized();
    }

    private bool TagTypesExists(int id)
    {
        return _context.Roles.Any(e => e.Id == id);
    }
}