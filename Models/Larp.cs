using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Larp
{
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public string Shortname { get; set; }

    public string Location { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<LarpplayerCharacterSheetAllowed> LarpplayerCharacterSheetAlloweds { get; } =
        new List<LarpplayerCharacterSheetAllowed>();

    public virtual ICollection<LarpplayerCharacterSheetDisllowed> LarpplayerCharacterSheetDislloweds { get; } =
        new List<LarpplayerCharacterSheetDisllowed>();

    public virtual ICollection<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; } =
        new List<LarpplayerSeriesAllowed>();

    public virtual ICollection<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; } =
        new List<LarpplayerSeriesDisllowed>();

    public virtual ICollection<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; } = new List<LarpplayerTagAllowed>();

    public virtual ICollection<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; } =
        new List<LarpplayerTagDisllowed>();

    public virtual ICollection<Larprun> Larpruns { get; } = new List<Larprun>();

    public virtual ICollection<Larptag> Larptags { get; } = new List<Larptag>();

    public virtual ICollection<UserLarprole> UserLarproles { get; } = new List<UserLarprole>();
}