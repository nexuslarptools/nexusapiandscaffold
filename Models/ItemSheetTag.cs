using System;

namespace NEXUSDataLayerScaffold.Models;

public class ItemSheetTag
{
    public int Id { get; set; }

    public int ItemsheetId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual ItemSheet Itemsheet { get; set; }

    public virtual Tag Tag { get; set; }
}