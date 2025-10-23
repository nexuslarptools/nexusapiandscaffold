using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Enums;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Logic;

public class UsersLogic
{
    private readonly NexusLarpLocalContext _context;
    private static readonly ILogger Logger = LoggerFactory.Create(builder =>
    {
        // Use console provider; OpenTelemetry is already configured globally in Startup
        builder.AddConsole();
    }).CreateLogger<UsersLogic>();

    public UsersLogic(NexusLarpLocalContext context)
    {
        _context = context;
    }

    public static MetadataRoles GetUserAuth0Info(string authIdValue, NexusLarpLocalContext context)
    {
        Logger.LogInformation("GetUserAuth0Info (DB-backed) started for authId {AuthId}", authIdValue);
        try
        {
            var localUser = context.Users
                .Include(u => u.UserLarproles)
                .ThenInclude(ulr => ulr.Role)
                .Where(u => u.Authid == authIdValue && u.Isactive == true)
                .FirstOrDefault();

            if (localUser != null)
            {
                var roles = new MetadataRoles(localUser);
                Logger.LogInformation("GetUserAuth0Info (DB-backed) built roles for {AuthId}", authIdValue);
                return roles;
            }
            else
            {
                Logger.LogWarning("GetUserAuth0Info (DB-backed): No local user found for {AuthId}", authIdValue);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "GetUserAuth0Info (DB-backed) failed for {AuthId}", authIdValue);
        }
        return new MetadataRoles();
    }

    public static bool IsUserAuthed(string authIdValue, string accessToken, string authLevel,
        NexusLarpLocalContext _context)
    {
        Logger.LogInformation("IsUserAuthed called for {AuthId} with level '{Level}'", authIdValue, authLevel);
        //var curruser = GetUserInfoByAuth(authIdValue).Result;
        var foundUsers = _context.Users.Where(u => u.Authid == authIdValue).ToList();
        Logger.LogDebug("Found {Count} user records for {AuthId}", foundUsers.Count, authIdValue);

        if (foundUsers.Count > 1 && foundUsers.Any(fu => fu.Isactive == true))
        {
            Logger.LogWarning("Multiple active user records detected for {AuthId}. Consolidating...", authIdValue);
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
            Logger.LogInformation("User {AuthId} not found locally. Attempting to create from access token.", authIdValue);
            var autheduser = GetUserInfo(accessToken, _context);

            if (!string.IsNullOrEmpty(autheduser.Result.authid))
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
                    Logger.LogInformation("Created new user record for {AuthId}", authIdValue);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to create new user record for {AuthId}", authIdValue);
                }
            }

            return false;
        }

        if (foundUser.Isactive == false)
        {
            Logger.LogWarning("User {AuthId} is inactive", authIdValue);
            if (authIdValue == "auth0|5eb6c2556b69bc0c120737e9")
            {
                foundUser.Isactive = true;
                _context.Users.Update(foundUser);
                _context.SaveChanges();
                Logger.LogInformation("Admin override: reactivated user {AuthId}", authIdValue);
                return true;
            }

            return false;
        }

        if (authLevel == string.Empty)
        {
            Logger.LogDebug("No auth level required; granting access for {AuthId}", authIdValue);
            return true;
        }

        var roleNum = _context.Roles.Where(r => r.Rolename == authLevel).Select(r => r.Ord).FirstOrDefault();

        var foundrole = _context.UserLarproles
            .Where(ulr => ulr.Userguid == foundUser.Guid && ulr.Role.Ord >= roleNum && ulr.Isactive == true)
            .FirstOrDefault();

        if (foundrole == null)
        {
            Logger.LogWarning("User {AuthId} lacks required role {Level}", authIdValue, authLevel);
            if (authIdValue == "auth0|5eb6c2556b69bc0c120737e9")
            {
                Logger.LogInformation("Admin override path for {AuthId} and level {Level}", authIdValue, authLevel);
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
                    Logger.LogInformation("Admin override: created role mapping for {AuthId} to {Level}", authIdValue, authLevel);
                }
                else
                {
                    foundrole.Isactive = true;
                    _context.UserLarproles.Update(foundrole);
                    Logger.LogInformation("Admin override: reactivated role mapping for {AuthId} to {Level}", authIdValue, authLevel);
                }

                _context.SaveChanges();
            }

            return false;
        }

        Logger.LogInformation("Access granted for {AuthId} at level {Level}", authIdValue, authLevel);
        return true;
    }


    public static async Task<AuthUser> GetUserInfo(string accessToken2, NexusLarpLocalContext _context)
    {
        Logger.LogInformation("GetUserInfo (JWT parse) called");
        var returnuser = new AuthUser();

        try
        {
            // Try to parse JWT locally (no external calls). This does not validate signature.
            if (string.IsNullOrWhiteSpace(accessToken2)) return returnuser;
            var parts = accessToken2.Split('.');
            if (parts.Length < 2) return returnuser;
            static string Base64UrlDecode(string s)
            {
                s = s.Replace('-', '+').Replace('_', '/');
                switch (s.Length % 4)
                {
                    case 2: s += "=="; break;
                    case 3: s += "="; break;
                }
                var bytes = Convert.FromBase64String(s);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            var payload = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (root.TryGetProperty("sub", out var sub)) returnuser.authid = sub.GetString();
            if (root.TryGetProperty("name", out var name)) returnuser.name = name.GetString();
            // Prefer standard email; fallback to custom namespace if present
            if (root.TryGetProperty("email", out var email)) returnuser.email = email.GetString();
            else if (root.TryGetProperty("https://NexusLarps.com/email", out var nemail)) returnuser.email = nemail.GetString();

            // Optional roles/permissions arrays: support standard 'roles' and custom namespaces
            var roles = new List<string>();
            if (root.TryGetProperty("roles", out var rolesEl) && rolesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in rolesEl.EnumerateArray()) roles.Add(r.GetString() ?? string.Empty);
            }
            else if (root.TryGetProperty("https://NexusLarps.com/roles", out var nrEl) && nrEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in nrEl.EnumerateArray()) roles.Add(r.GetString() ?? string.Empty);
            }
            returnuser.roles = roles;

            var perms = new List<string>();
            if (root.TryGetProperty("permissions", out var pEl) && pEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in pEl.EnumerateArray()) perms.Add(p.GetString() ?? string.Empty);
            }
            else if (root.TryGetProperty("https://NexusLarps.com/permissions", out var npEl) && npEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in npEl.EnumerateArray()) perms.Add(p.GetString() ?? string.Empty);
            }
            returnuser.permissions = perms;

            Logger.LogInformation("GetUserInfo (JWT parse) success for subject {Sub}", returnuser.authid);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "GetUserInfo (JWT parse) failed");
        }

        // simulate async signature
        await Task.CompletedTask;
        return returnuser;
    }

    public static List<Guid?> GetUserTagsList(string authIdValue, NexusLarpLocalContext _context, string level,
        bool isWizard)
    {
        Logger.LogInformation("GetUserTagsList for {AuthId} level {Level} (isWizard={IsWizard})", authIdValue, level, isWizard);
        if (isWizard)
        {
            var allTags = _context.Larptags.Where(lt => lt.Isactive == true && lt.Larpguid != null).Select(lt => lt.Tagguid)
                .ToList();
            Logger.LogDebug("GetUserTagsList returning all tags count={Count} for wizard", allTags.Count);
            return allTags;
        }

        var returnlist = new List<Guid?>();

        var querylevel = _context.Roles.Where(r => r.Rolename == level).FirstOrDefault();

        var userLarps = _context.UserLarproles.Where(ulr =>
                ulr.Isactive == true && ulr.User.Authid == authIdValue && ulr.Role.Ord >= querylevel.Ord)
            .Select(ulr => ulr.Larpguid).ToList();

        returnlist = _context.Larptags.Where(lt => lt.Isactive == true && userLarps.Contains(lt.Larpguid))
            .Select(lt => lt.Tagguid).ToList();

        Logger.LogDebug("GetUserTagsList returning {Count} tags for {AuthId}", returnlist.Count, authIdValue);
        return returnlist;
    }

    public static async Task<Guid> GetUserGuid(string authIdValue, NexusLarpLocalContext _context)
    {
        Logger.LogDebug("GetUserGuid for {AuthId}", authIdValue);
        return await _context.Users.Where(u => u.Authid == authIdValue).Select(u => u.Guid).FirstOrDefaultAsync();
    }

    public static async void UpdateAuth0User(string email, MetadataRoles roles)
    {
        Logger.LogInformation("UpdateAuth0User called for email {Email}", email);
        try
        {
            var aLogic = new AuthLogic();
            var usersemail = aLogic.GetAllUsersByEmail(email);

            foreach (var u in usersemail.Result)
            {
                var usr = aLogic.UpdateUserRoles(u.UserId, roles);
                Logger.LogDebug("Updated roles for Auth0 user {UserId}", u.UserId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "UpdateAuth0User failed for {Email}", email);
        }
    }

    public static async Task<Auth0.ManagementApi.Models.User> GetUserInfoByAuth(string authIdValue)
    {
        Logger.LogDebug("GetUserInfoByAuth for {AuthId}", authIdValue);
        var aLogic = new AuthLogic();
        return await aLogic.GetUserByAuthID(authIdValue);
    }

}
