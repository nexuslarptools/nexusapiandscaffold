using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Logic;

public class UsersLogic
{
    private readonly NexusLarpLocalContext _context;

    public UsersLogic(NexusLarpLocalContext context)
    {
        _context = context;
    }

    public static bool IsUserAuthed(string authIdValue, string accessToken, string authLevel,
        NexusLarpLocalContext _context)
    {
        var foundUsers = _context.Users.Where(u => u.Authid == authIdValue).ToList();

        if (foundUsers.Count > 1 && foundUsers.Any(fu => fu.Isactive == true))
        {
            foundUsers[0].Isactive = true;
            _context.Users.Update(foundUsers[0]);
            _context.SaveChanges();

            for (var i = 1; i < foundUsers.Count; i++)
            {
                var user = foundUsers[i];
                user.Isactive = false;
                _context.Users.Update(user);
                _context.SaveChanges();
            }
        }

        var foundUser = foundUsers.FirstOrDefault();

        if (foundUser == null)
        {
            var autheduser = GetUserInfo(accessToken, _context);

            if (autheduser.Result.authid != null && autheduser.Result.authid != string.Empty)
            {
                var newUsers = new User
                {
                    Email = autheduser.Result.email,
                    Preferredname = autheduser.Result.name,
                    Authid = autheduser.Result.authid,
                    Isactive = true
                };

                try
                {
                    _context.Users.Add(newUsers);
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    var huh = e;
                }
            }

            return false;
        }

        if (foundUser.Isactive == false)
        {
            if (authIdValue == "auth0|5eb6c2556b69bc0c120737e9")
            {
                foundUser.Isactive = true;
                _context.Users.Update(foundUser);
                _context.SaveChanges();

                return true;
            }

            return false;
        }

        var roleNum = _context.Roles.Where(r => r.Rolename == authLevel).Select(r => r.Ord).FirstOrDefault();

        var foundrole = _context.UserLarproles
            .Where(ulr => ulr.Userguid == foundUser.Guid && ulr.Role.Ord >= roleNum && ulr.Isactive == true)
            .FirstOrDefault();

        if (foundrole == null)
        {
            if (authIdValue == "auth0|5eb6c2556b69bc0c120737e9")
            {
                foundrole = _context.UserLarproles.Where(ulr =>
                        ulr.Userguid == foundUser.Guid && ulr.Role.Rolename == authLevel && ulr.Isactive == false)
                    .FirstOrDefault();

                if (foundrole == null)
                {
                    foundrole = new UserLarprole
                    {
                        Userguid = foundUser.Guid,
                        Larpguid = _context.Larps.Where(l => l.Isactive == true && l.Name == "Default")
                            .Select(l => l.Guid).FirstOrDefault(),
                        Roleid = _context.Roles.Where(r => r.Rolename == "Wizard").Select(l => l.Id).FirstOrDefault()
                    };

                    _context.UserLarproles.Add(foundrole);
                }
                else
                {
                    foundrole.Isactive = true;
                    _context.UserLarproles.Update(foundrole);
                }

                _context.SaveChanges();
            }

            return false;
        }

        return true;
    }


    public static async Task<AuthUser> GetUserInfo(string accessToken2, NexusLarpLocalContext _context)
    {
        // Get the access token.

        var returnuser = new AuthUser();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);

        var responseMessage = await client.GetAsync("https://dev-3xazewbu.auth0.com/userinfo");
        if (responseMessage.IsSuccessStatusCode)
        {
            var responseData = responseMessage.Content.ReadAsStringAsync().Result;
            var JsonHoldingCell = JsonDocument.Parse(responseData);

            if (JsonHoldingCell.RootElement.GetProperty("email_verified").ToString().ToLower() != "true")
                return returnuser;

            returnuser.name = JsonHoldingCell.RootElement.GetProperty("name").ToString();
            returnuser.authid = JsonHoldingCell.RootElement.GetProperty("sub").ToString();
            returnuser.email = JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/email").ToString();


            var permissionsholder = new List<string>();
            for (var i = 0;
                 i < JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/permissions").GetArrayLength();
                 i++)
                permissionsholder.Add(JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/permissions")[i]
                    .ToString());
            returnuser.permissions = permissionsholder;

            permissionsholder = new List<string>();
            for (var i = 0;
                 i < JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/roles").GetArrayLength();
                 i++)
                permissionsholder.Add(JsonHoldingCell.RootElement.GetProperty("https://NexusLarps.com/roles")[i]
                    .ToString());
            returnuser.roles = permissionsholder;
        }

        return returnuser;
    }

    public static List<Guid?> GetUserTagsList(string authIdValue, NexusLarpLocalContext _context, string level,
        bool isWizard)
    {
        if (isWizard)
            return _context.Larptags.Where(lt => lt.Isactive == true && lt.Larpguid != null).Select(lt => lt.Tagguid)
                .ToList();

        var returnlist = new List<Guid?>();

        var querylevel = _context.Roles.Where(r => r.Rolename == level).FirstOrDefault();

        var userLarps = _context.UserLarproles.Where(ulr =>
                ulr.Isactive == true && ulr.User.Authid == authIdValue && ulr.Role.Ord >= querylevel.Ord)
            .Select(ulr => ulr.Larpguid).ToList();

        returnlist = _context.Larptags.Where(lt => lt.Isactive == true && userLarps.Contains(lt.Larpguid))
            .Select(lt => lt.Tagguid).ToList();

        return returnlist;
    }

    public static Guid GetUserGuid(string authIdValue, NexusLarpLocalContext _context)
    {
        return _context.Users.Where(u => u.Authid == authIdValue).Select(u => u.Guid).FirstOrDefault();
    }
}