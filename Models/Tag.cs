using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class Tag
{
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public Guid Tagtypeguid { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; } = new List<LarpplayerTagAllowed>();

    public virtual ICollection<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; } = new List<LarpplayerTagDisllowed>();

    public virtual ICollection<Larptag> Larptags { get; } = new List<Larptag>();

    public virtual TagType Tagtype { get; set; }
}
