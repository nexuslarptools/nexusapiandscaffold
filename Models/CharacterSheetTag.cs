using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class CharacterSheetTag
{
    public int Id { get; set; }

    public int CharactersheetId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual CharacterSheet Charactersheet { get; set; }

    public virtual Tag Tag { get; set; }
}
