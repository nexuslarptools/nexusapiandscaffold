using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class CharacterSheetApprovedTag
{
    public int Id { get; set; }

    public int CharactersheetapprovedId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual CharacterSheetApproved Charactersheetapproved { get; set; }

    public virtual Tag Tag { get; set; }
}
