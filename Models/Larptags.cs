using System;

namespace NEXUSDataLayerScaffold.Models;

public class Larptags
{
    public int Id { get; set; }
    public Guid? Tagguid { get; set; }
    public Guid? Larpguid { get; set; }
    public bool? Isactive { get; set; }

    public virtual Larps Larpgu { get; set; }
    public virtual Tags Taggu { get; set; }
}