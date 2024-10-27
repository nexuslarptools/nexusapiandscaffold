using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public UsersController(NexusLarpLocalContext context)
    {
        _context = context;
    }


    public static string GetUser(HttpContext httpContext)
    {
        var email = "";

        return email;
    }


    public static bool UserPermissionAuth(AuthUser user, string authName)
    {
        if (user.permissions != null)
            if (user.permissions.Contains(authName))
                return true;

        return false;
    }

    /// <summary>
    ///     Gets a list of all users.
    /// </summary>
    /// <returns></returns>
    // GET api/v1/Users
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserOut>>> GetAllUsers()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            // var UsersList = _context.Users.Select(u => new UserOut {Guid=u.Guid, Firstname=u.Firstname, Lastname=u.Lastname,
            // Preferredname=u.Preferredname, Email=u.Email, Pronounsguid=u.Pronounsguid
            // }).ToList();
            var UserListOut = new List<UserOut>();
            var UsersList = _context.Users.Where(u => u.Isactive == true).ToList();
            var UsersRolesList = _context.UserLarproles.Where(ulr => ulr.Isactive == true).ToList();
            var RolesList = _context.Roles.ToList();
            var LarpsList = _context.Larps.Where(l => l.Isactive == true).ToList();
            var pronounsList = _context.Pronouns.ToList();

            var rolefirst = new RoleIDFirst();
            Comparer<RoleOut> rc = rolefirst;

            foreach (var user in UsersList)
            {
                var newout = new UserOut
                {
                    Guid = user.Guid,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Preferredname = user.Preferredname,
                    Email = user.Email,
                    Pronounsguid = user.Pronounsguid
                };
                if (user.Pronounsguid != null)
                {
                    newout.Pronouns = pronounsList.Where(pn => pn.Guid == user.Pronounsguid)
                        .FirstOrDefault().Pronouns;
                }
                foreach (var larprole in UsersRolesList)
                    if (larprole.Userguid == newout.Guid)
                    {
                        if (!newout.LarpRoles.Any(lr => lr.LarpGuid == larprole.Larpguid))
                        {
                            var newULR = new UserLarpRoleOut
                            {
                                LarpGuid = larprole.Larpguid,
                                LarpName = LarpsList.Where(ll => ll.Guid == larprole.Larpguid).Select(ll => ll.Name)
                                    .FirstOrDefault()
                            };

                            newout.LarpRoles.Add(newULR);
                        }

                        var currLARP = newout.LarpRoles.Where(lr => lr.LarpGuid == larprole.Larpguid)
                            .FirstOrDefault();

                        var newRole = RolesList.Where(rl => rl.Id == larprole.Roleid).FirstOrDefault();

                        var newRoleOut = new RoleOut((int)newRole.Ord, newRole.Rolename);

                        currLARP.Roles.Add(newRoleOut);

                        var sortedroles = from role in currLARP.Roles
                            orderby role.RoleID
                            select role;

                        currLARP.Roles = sortedroles.ToList();
                    }

                UserListOut.Add(newout);
            }

            var sortedusers = from user in UserListOut
                orderby user.Email
                select user;

            UserListOut = sortedusers.ToList();

            return Ok(UserListOut);
        }

        return Ok("Not Authorized");
    }

    [HttpGet("WithInactive")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserOut>>> GetAllUsersWithInactive()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            // var UsersList = _context.Users.Select(u => new UserOut {Guid=u.Guid, Firstname=u.Firstname, Lastname=u.Lastname,
            // Preferredname=u.Preferredname, Email=u.Email, Pronounsguid=u.Pronounsguid
            // }).ToList();
            var UserListOut = new List<UserOut>();
            var UsersList = _context.Users.ToList();
            var UsersRolesList = _context.UserLarproles.Where(ulr => ulr.Isactive == true).ToList();
            var RolesList = _context.Roles.ToList();
            var LarpsList = _context.Larps.Where(l => l.Isactive == true).ToList();

            var rolefirst = new RoleIDFirst();
            Comparer<RoleOut> rc = rolefirst;

            foreach (var user in UsersList)
            {
                var newout = new UserOut
                {
                    Guid = user.Guid,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Preferredname = user.Preferredname,
                    Email = user.Email,
                    Pronounsguid = user.Pronounsguid
                };
                foreach (var larprole in UsersRolesList)
                    if (larprole.Userguid == newout.Guid)
                    {
                        if (!newout.LarpRoles.Any(lr => lr.LarpGuid == larprole.Larpguid))
                        {
                            var newULR = new UserLarpRoleOut
                            {
                                LarpGuid = larprole.Larpguid,
                                LarpName = LarpsList.Where(ll => ll.Guid == larprole.Larpguid).Select(ll => ll.Name)
                                    .FirstOrDefault()
                            };

                            newout.LarpRoles.Add(newULR);
                        }

                        var currLARP = newout.LarpRoles.Where(lr => lr.LarpGuid == larprole.Larpguid)
                            .FirstOrDefault();

                        var newRole = RolesList.Where(rl => rl.Id == larprole.Roleid).FirstOrDefault();

                        var newRoleOut = new RoleOut((int)newRole.Ord, newRole.Rolename);

                        currLARP.Roles.Add(newRoleOut);

                        var sortedroles = from role in currLARP.Roles
                            orderby role.RoleID
                            select role;

                        currLARP.Roles = sortedroles.ToList();
                    }

                UserListOut.Add(newout);
            }

            var sortedusers = from user in UserListOut
                orderby user.Email
                select user;

            UserListOut = sortedusers.ToList();

            return Ok(UserListOut);
        }

        return Ok("Not Authorized");
    }

    /// <summary>
    ///     Gets a single user's information
    /// </summary>
    /// <returns></returns>
    // GET api/v1/Users/{id}
    [HttpGet("{id}")]
    [Authorize]
    public ActionResult<UserOut> GetAUser(Guid id)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        var currUseGuid = _context.Users.Where(u => u.Authid == authId && u.Isactive == true).FirstOrDefault().Guid;

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context) ||
            currUseGuid == id)
        {
            var user = _context.Users.Where(u => u.Guid == id).Include("Pronouns").FirstOrDefault();
            if (user == null) { return NoContent(); }

            var UsersRolesList = _context.UserLarproles.Where(ulr => ulr.Isactive == true).ToList();
            var RolesList = _context.Roles.ToList();
            var LarpsList = _context.Larps.Where(l => l.Isactive == true).ToList();

            var newout = new UserOut
            {
                Guid = user.Guid,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Preferredname = user.Preferredname,
                Email = user.Email,
                Pronounsguid = user.Pronounsguid,
                Pronouns = user.Pronouns.Pronouns,
            };
            foreach (var larprole in UsersRolesList)
                if (larprole.Userguid == newout.Guid)
                {
                    if (!newout.LarpRoles.Any(lr => lr.LarpGuid == larprole.Larpguid))
                    {
                        var newULR = new UserLarpRoleOut
                        {
                            LarpGuid = larprole.Larpguid,
                            LarpName = LarpsList.Where(ll => ll.Guid == larprole.Larpguid).Select(ll => ll.Name)
                                .FirstOrDefault()
                        };

                        newout.LarpRoles.Add(newULR);
                    }

                    var currLARP = newout.LarpRoles.Where(lr => lr.LarpGuid == larprole.Larpguid).FirstOrDefault();

                    var newRole = RolesList.Where(rl => rl.Id == larprole.Roleid).FirstOrDefault();

                    var newRoleOut = new RoleOut((int)newRole.Ord, newRole.Rolename);

                    currLARP.Roles.Add(newRoleOut);
                }


            return Ok(newout);
        }

        return Ok("Not Authorized");
    }

    /// <summary>
    /// Gets a single user's information
    /// </summary>
    /// <returns></returns>
    // GET api/v1/Users/{id}
    [HttpGet("Permission")]
    [Authorize]
    public ActionResult<string> GetCurrentUserAuth()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (!UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context)) return Ok("{\"AuthLevel\":\"NONE\"}");


        var Rolelist = _context.UserLarproles.Where(u => u.User.Authid == authId && u.Isactive == true)
            .Include("Role").ToList();

        var authlevel = 0;
        foreach (var role in Rolelist)
            if (role.Role.Ord > authlevel)
                authlevel = (int)role.Role.Ord;

        if (authlevel == 0) return Ok("{\"AuthLevel\":\"None\"}");

        var maxrole = _context.Roles.Where(r => r.Ord == authlevel).Select(r => r.Rolename.Replace(" ", ""))
            .FirstOrDefault();

        return Ok("{\"AuthLevel\":\"" + maxrole + "\"}");
    }

    /// <summary>
    /// Gets a single user's information
    /// </summary>
    /// <returns></returns>
    // GET api/v1/Users/{id}
    [HttpGet("CurrentGuid")]
    public ActionResult<Guid> GetCurrentUserGuid()
    {
        return Guid.Parse("e6dc1ae4-b99d-11ee-be57-5f6ca0b9cfd5");
    }


    // PUT: api/v1/users/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize]
    public async Task<IActionResult> PutUser(Guid guid, Users user)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))

        if (guid != user.Guid) return BadRequest();

        var oldUserinfo = _context.Users.Where(u => u.Guid == user.Guid).FirstOrDefault();

        var curUser = _context.Users.Where(u => u.Authid == authId).FirstOrDefault();
        if (user.Guid != user.Guid && !UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            return Unauthorized();
        }

        if (oldUserinfo == null) return NotFound();

        // change all dffering info.
        if (oldUserinfo.Firstname != user.Firstname && user.Firstname != null)
            oldUserinfo.Firstname = user.Firstname;

        if (oldUserinfo.Lastname != user.Lastname && user.Lastname != null) oldUserinfo.Lastname = user.Lastname;

        if (oldUserinfo.Preferredname != user.Preferredname && user.Preferredname != null)
            oldUserinfo.Preferredname = user.Preferredname;

        if (oldUserinfo.Pronounsguid != user.Pronounsguid) oldUserinfo.Pronounsguid = user.Pronounsguid;

        if (UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            //check if any user larp roles were added
            if (user.UserLarproles != null)
            {
                var currroles = await _context.UserLarproles
                    .Where(ulr => ulr.Userguid == guid && ulr.Isactive == true).ToListAsync();
                var foundid = new List<int>();

                foreach (var larpRole in user.UserLarproles)
                {
                    //Check if the role eixsts already.
                    var foundrole = await _context.UserLarproles.Where(ulr => ulr.Userguid == guid &&
                                                                              ulr.Larpguid == larpRole.Larpguid &&
                                                                              ulr.Role.Ord == larpRole.Role.Id)
                        .FirstOrDefaultAsync();

                    // if it doesn't make a new userlaprrole.
                    if (foundrole == null)
                    {
                        var actualroleID = _context.Roles.Where(r => r.Ord == larpRole.Role.Id).FirstOrDefault().Id;

                        foundrole = new UserLarprole
                        {
                            Roleid = actualroleID,
                            Larpguid = larpRole.Larpguid,
                            Userguid = guid,
                            Isactive = true
                        };

                        _context.UserLarproles.Add(foundrole);
                    }
                    else
                    {
                        foundrole.Isactive = true;
                        _context.UserLarproles.Update(foundrole);
                    }

                    if (currroles.Any(cr => cr.Larpguid == foundrole.Larpguid && cr.Roleid == foundrole.Roleid))
                        foundid.Add(currroles
                            .Where(cr => cr.Larpguid == foundrole.Larpguid && cr.Roleid == foundrole.Roleid)
                            .Select(cr => cr.Id).FirstOrDefault());
                }

                foreach (var currRole in currroles)
                    if (!foundid.Contains(currRole.Id))
                    {
                        currRole.Isactive = false;
                        _context.UserLarproles.Update(currRole);
                    }
            }
            else
            {
                //remove all UserLarpRoles
                var foundroles = await _context.UserLarproles.Where(ulr => ulr.Userguid == guid).ToListAsync();
                foreach (var fndrole in foundroles)
                {
                    fndrole.Isactive = false;
                    _context.UserLarproles.Update(fndrole);
                }
            }

            _context.Users.Update(oldUserinfo);
        }

        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPut("DeactivateUser/{guid}")]
    [Authorize]
    public async Task<ActionResult<UserOut>> Deactivate(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var user = await _context.Users.Where(u => u.Guid == guid).FirstOrDefaultAsync();

            user.Isactive = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            var output = new UserOut
            {
                Guid = user.Guid,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Preferredname = user.Preferredname,
                Email = user.Email,
                Pronounsguid = user.Pronounsguid,
                Discordname = user.Discordname
            };
            return Ok(output);
        }

        return Unauthorized();
    }

    [HttpPut("ActivateUser/{guid}")]
    [Authorize]
    public async Task<ActionResult<UserOut>> Reactivate(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var user = await _context.Users.Where(u => u.Guid == guid).FirstOrDefaultAsync();

            user.Isactive = true;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            var output = new UserOut
            {
                Guid = user.Guid,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Preferredname = user.Preferredname,
                Email = user.Email,
                Pronounsguid = user.Pronounsguid,
                Discordname = user.Discordname
            };
            return Ok(output);
        }

        return Unauthorized();
    }


    // PUT: api/v1/users/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost("AddUserRole/{guid}")]
    [Authorize]
    public async Task<IActionResult> PostUserRole(Guid guid, UserRoleInput user)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            if (guid != user.Guid) return BadRequest();

            var oldUserinfo = _context.Users.Where(u => u.Guid == user.Guid).FirstOrDefault();

            if (oldUserinfo == null) return NotFound();

            var roleinfo = _context.Roles.Where(r => r.Rolename == user.RoleName.Replace(" ", "")).FirstOrDefault();

            if (roleinfo == null) return NotFound();


            if ((roleinfo.Rolename == "HeadGM" || roleinfo.Rolename == "Wizard") &&
                !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context)) return Unauthorized();


            var larpInfo = _context.Larps.Where(u => u.Guid == user.LarpGuid).FirstOrDefault();

            if (larpInfo == null && user.LarpGuid != null) return NotFound();

            var userLARPRoleInfo = _context.UserLarproles.Where(ulr =>
                (larpInfo == null || ulr.Larpguid == larpInfo.Guid) && ulr.Userguid == oldUserinfo.Guid
                                                                    && ulr.Roleid == roleinfo.Ord).FirstOrDefault();

            if (userLARPRoleInfo == null)
            {
                var newUserLARPRole = new UserLarprole
                {
                    Userguid = oldUserinfo.Guid,
                    Larpguid = larpInfo.Guid,
                    Roleid = roleinfo.Id
                };
                _context.UserLarproles.Add(newUserLARPRole);
            }
            else
            {
                if (userLARPRoleInfo.Isactive == true) return BadRequest();

                userLARPRoleInfo.Isactive = true;
                _context.UserLarproles.Update(userLARPRoleInfo);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        return Unauthorized();
    }


    // PUT: api/v1/users/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost("RemoveUserRole/{guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveUserRole(Guid guid, UserRoleInput user)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            if (guid != user.Guid) return BadRequest();

            var roleinfo = _context.Roles.Where(r => r.Rolename == user.RoleName.Replace(" ", "")).FirstOrDefault();

            if (roleinfo == null) return NotFound();

            if ((roleinfo.Rolename == "HeadGM" || roleinfo.Rolename == "Wizard") &&
                !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context)) return Unauthorized();

            var userLARPRoleInfo = _context.UserLarproles.Where(ulr => ulr.Larpguid == user.LarpGuid &&
                                                                       ulr.Userguid == user.Guid
                                                                       && ulr.Roleid == roleinfo.Ord).FirstOrDefault();

            if (userLARPRoleInfo == null) return NotFound();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context) &&
                (roleinfo.Rolename == "Wizard" || roleinfo.Rolename == "HeadGM")) return Unauthorized();


            if (userLARPRoleInfo.Isactive == false) return BadRequest();

            userLARPRoleInfo.Isactive = false;
            _context.UserLarproles.Update(userLARPRoleInfo);


            await _context.SaveChangesAsync();

            return NoContent();
        }

        return Unauthorized();
    }
}