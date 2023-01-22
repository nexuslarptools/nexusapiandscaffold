using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class UserOut
    {
        public UserOut()
        {
            LarpRoles = new List<UserLarpRoleOut>();
            EffectiveRole = new RoleOut();
        }

        public UserOut(Guid guid, string firstname, string lastname, string preferedName, string email, Guid? pronounsguid, string discordName
        , RoleOut userRoles)
        {
            EffectiveRole = userRoles;
            this.Guid = guid;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Preferredname = preferedName;
            this.Email = email;
            this.Pronounsguid = pronounsguid;
            this.Discordname = discordName;
        }

        public Guid Guid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Preferredname { get; set; }
        public string Email { get; set; }
        public Guid? Pronounsguid { get; set; }
        public string Discordname { get; set; }

        public List<UserLarpRoleOut> LarpRoles { get; set; }
        public RoleOut EffectiveRole { get; set; }

    }
}
