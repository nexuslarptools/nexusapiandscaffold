using System;
using System.Linq;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Tests.Infrastructure;

public static class TestDataSeeder
{
    public static void Seed(NexusLarpLocalContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.Add(new Role { Rolename = "Reader", Ord = 1 });
            db.Roles.Add(new Role { Rolename = "Writer", Ord = 2 });
            db.Roles.Add(new Role { Rolename = "Approver", Ord = 3 });
            db.Roles.Add(new Role { Rolename = "HeadGM", Ord = 4 });
            db.Roles.Add(new Role { Rolename = "Wizard", Ord = 5 });
            db.SaveChanges();
        }

        // Seed a Wizard user with a Wizard role assignment (so UsersLogic.IsUserAuthed works)
        var wizardAuthId = "auth0|wizard";
        var wizardUser = db.Users.FirstOrDefault(u => u.Authid == wizardAuthId);
        if (wizardUser == null)
        {
            wizardUser = new User
            {
                Guid = Guid.NewGuid(),
                Firstname = "Wizard",
                Lastname = "User",
                Email = "wizard@example.com",
                Authid = wizardAuthId,
                Isactive = true
            };
            db.Users.Add(wizardUser);
            db.SaveChanges();
        }

        var wizardRole = db.Roles.First(r => r.Rolename == "Wizard");
        if (!db.UserLarproles.Any(ulr => ulr.Userguid == wizardUser.Guid && ulr.Roleid == wizardRole.Id && ulr.Isactive == true))
        {
            db.UserLarproles.Add(new UserLarprole
            {
                Userguid = wizardUser.Guid,
                Roleid = wizardRole.Id,
                Larpguid = null,
                Isactive = true
            });
            db.SaveChanges();
        }

        // Also add a HeadGM user (no DB role assignment required for policy-only test)
        var hgm = db.Users.FirstOrDefault(u => u.Authid == "auth0|headgm");
        if (hgm == null)
        {
            db.Users.Add(new User
            {
                Guid = Guid.NewGuid(),
                Firstname = "Head",
                Lastname = "GM",
                Email = "headgm@example.com",
                Authid = "auth0|headgm",
                Isactive = true
            });
            db.SaveChanges();
        }
    }
}
