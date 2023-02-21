using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Larps
{
    public Larps()
    {
        Larptags = new HashSet<Larptags>();
        UserLarproles = new HashSet<UserLarproles>();
    }

    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Shortname { get; set; }
    public string Location { get; set; }
    public bool? Isactive { get; set; }

    public virtual ICollection<Larptags> Larptags { get; set; }
    public virtual ICollection<UserLarproles> UserLarproles { get; set; }
}