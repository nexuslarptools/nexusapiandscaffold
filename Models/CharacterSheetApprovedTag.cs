using System;

namespace NEXUSDataLayerScaffold.Models;

public class CharacterSheetApprovedTag
{
    public int Id { get; set; }

    public int CharactersheetapprovedId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual CharacterSheetApproved Charactersheetapproved { get; set; }

    public virtual Tag Tag { get; set; }
}