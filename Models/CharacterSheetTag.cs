using System;

namespace NEXUSDataLayerScaffold.Models;

public class CharacterSheetTag
{
    public int Id { get; set; }

    public int CharactersheetId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual CharacterSheet Charactersheet { get; set; }

    public virtual Tag Tag { get; set; }
}