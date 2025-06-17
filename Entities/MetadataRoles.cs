using NEXUSDataLayerScaffold.Enums;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEXUSDataLayerScaffold.Entities
{
    public class MetadataRoles
    {

        public MetadataRoles()
        {
            authorization = new Authorization();
        }

        public MetadataRoles(User user)
        {
            authorization = new Authorization()
            {
                groups = new List<string>(),
                roles = new List<Permission>()
            };

            foreach(var l in user.UserLarproles)
            {
                if (l.Larpguid != null)
                {
                    if (!authorization.groups.Contains(l.Larpguid.ToString()))
                    {
                        authorization.groups.Add(l.Larpguid.ToString());
                        authorization.roles.Add(new Permission()
                        {
                            LARP = l.Larpguid.ToString(),
                            permissions = new List<int>()
                            {
                                (int)l.Role.Ord
                            }
                        });
                    }
                    else
                    {
                        authorization.roles.Where(r => r.LARP == l.Larpguid.ToString())
                            .FirstOrDefault().permissions.Add((int)l.Role.Ord);
                    }

                }

            }
        }

        public Authorization authorization { get; set; }

        public class Authorization
        {
            public List<string> groups { get; set; }
            public List<Permission> roles { get; set; }
        }

        public class Permission
        {
            public string LARP { get; set; }
            public List<int> permissions { get; set; }
        }

        public bool isAuthed(string authlevel)
        {
            int authInt = (int)Enum.Parse(typeof(AuthLevelEnum), authlevel);

            foreach(var role in authorization.roles)
            {
                if(role.permissions.Contains(authInt))
                {
                    return true;
                }
            }
            return false;
        }

        public List<Guid> GetAllowedLARPGuid()
        {
            var output = new List<Guid>();

            foreach (var role in authorization.roles)
            {
                output.Add(Guid.Parse(role.LARP));
            }

            return output;
        }
    }
}
