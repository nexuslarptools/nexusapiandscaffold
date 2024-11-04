using System;

namespace NEXUSDataLayerScaffold.Models;

public class Larptag
{
    public int Id { get; set; }

    public Guid? Tagguid { get; set; }

    public Guid? Larpguid { get; set; }

    public bool? Isactive { get; set; }

    public virtual Larp Larp { get; set; }

    public virtual Tag Tag { get; set; }
}