﻿using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class Roles
    {
        public Roles()
        {
            UserLarproles = new HashSet<UserLarproles>();
        }

        public int Id { get; set; }
        public string Rolename { get; set; }

        public virtual ICollection<UserLarproles> UserLarproles { get; set; }
    }
}