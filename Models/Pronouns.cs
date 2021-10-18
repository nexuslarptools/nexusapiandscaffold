using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class Pronouns
    {
        public Pronouns()
        {
            Users = new HashSet<Users>();
        }

        public Guid Guid { get; set; }
        public string Pronouns1 { get; set; }

        public virtual ICollection<Users> Users { get; set; }
    }
}
