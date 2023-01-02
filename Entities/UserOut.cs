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
        }

        public Guid Guid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Preferredname { get; set; }
        public string Email { get; set; }
        public Guid? Pronounsguid { get; set; }
        public string Discordname { get; set; }

        public List<UserLarpRoleOut> LarpRoles { get; set; }

    }
}
