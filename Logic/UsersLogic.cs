using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Enums;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Logic;

public class UsersLogic
{
    private readonly NexusLarpLocalContext _context;

    public UsersLogic(NexusLarpLocalContext context)
    {
        _context = context;
    }

    public static MetadataRoles GetUserAuth0Info(string authIdValue)
    {
        // Auth0 removed: return an empty MetadataRoles stub. Callers should not rely on external IdP.
        return new MetadataRoles();
    }

    public static bool IsUserAuthed(string authIdValue, string accessToken, string authLevel,
        NexusLarpLocalContext _context)
    {
        //var curruser = GetUserInfoByAuth(authIdValue).Result;
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

                if (authLevel == string.Empty)
                {
                    return true;
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
        // Auth0 removed: return a minimal stub user. In this stub, no external calls are made.
        await Task.CompletedTask;
        return new AuthUser();
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

    public static async Task<Guid> GetUserGuid(string authIdValue, NexusLarpLocalContext _context)
    {
        return await _context.Users.Where(u => u.Authid == authIdValue).Select(u => u.Guid).FirstOrDefaultAsync();
    }

    public static async void UpdateAuth0User(string email, MetadataRoles roles)
    {
        // Auth0 removed: stub method does nothing.
        await Task.CompletedTask;
    }

    public static async Task<object> GetUserInfoByAuth(string authIdValue)
    {
        // Auth0 removed: no user info available.
        await Task.CompletedTask;
        return null;
    }

}