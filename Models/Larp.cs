using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class Larp
{
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public string Shortname { get; set; }

    public string Location { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<LarpplayerCharacterSheetAllowed> LarpplayerCharacterSheetAlloweds { get; set; } = new List<LarpplayerCharacterSheetAllowed>();

    public virtual ICollection<LarpplayerCharacterSheetDisllowed> LarpplayerCharacterSheetDislloweds { get; set; } = new List<LarpplayerCharacterSheetDisllowed>();

    public virtual ICollection<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; set; } = new List<LarpplayerSeriesAllowed>();

    public virtual ICollection<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; set; } = new List<LarpplayerSeriesDisllowed>();

    public virtual ICollection<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; set; } = new List<LarpplayerTagAllowed>();

    public virtual ICollection<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; set; } = new List<LarpplayerTagDisllowed>();

    public virtual ICollection<Larprun> Larpruns { get; set; } = new List<Larprun>();

    public virtual ICollection<Larptag> Larptags { get; set; } = new List<Larptag>();

    public virtual ICollection<UserLarprole> UserLarproles { get; set; } = new List<UserLarprole>();
}
