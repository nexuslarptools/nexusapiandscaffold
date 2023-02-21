using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Tags
{
    public Tags()
    {
        Larptags = new HashSet<Larptags>();
    }

    public Guid Guid { get; set; }
    public string Name { get; set; }
    public Guid Tagtypeguid { get; set; }
    public bool? Isactive { get; set; }

    public virtual TagTypes Tagtypegu { get; set; }
    public virtual ICollection<Larptags> Larptags { get; set; }
}