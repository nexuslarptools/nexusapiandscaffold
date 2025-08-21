using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class Tag
{
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public Guid Tagtypeguid { get; set; }

    public bool Isactive { get; set; }

    public bool? Isapproved { get; set; }

    public Guid? ApprovedbyUserGuid { get; set; }

    public virtual ICollection<CharacterSheetApprovedTag> CharacterSheetApprovedTags { get; set; } = new List<CharacterSheetApprovedTag>();

    public virtual ICollection<CharacterSheetTag> CharacterSheetTags { get; set; } = new List<CharacterSheetTag>();

    public virtual ICollection<ItemSheetApprovedTag> ItemSheetApprovedTags { get; set; } = new List<ItemSheetApprovedTag>();

    public virtual ICollection<ItemSheetTag> ItemSheetTags { get; set; } = new List<ItemSheetTag>();

    public virtual ICollection<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; set; } = new List<LarpplayerTagAllowed>();

    public virtual ICollection<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; set; } = new List<LarpplayerTagDisllowed>();

    public virtual ICollection<Larptag> Larptags { get; set; } = new List<Larptag>();

    public virtual ICollection<SeriesTag> SeriesTags { get; set; } = new List<SeriesTag>();

    public virtual TagType Tagtype { get; set; }
}
