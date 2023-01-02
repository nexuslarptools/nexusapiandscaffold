using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class UserRoleInput
    {
        public Guid Guid { get; set; }

        public string RoleName { get; set; }

        public Guid? LarpGuid { get; set; }
    }
}
