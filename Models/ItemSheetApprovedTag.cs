using System;

namespace NEXUSDataLayerScaffold.Models;

public class ItemSheetApprovedTag
{
    public int Id { get; set; }

    public int ItemsheetapprovedId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual ItemSheetApproved Itemsheetapproved { get; set; }

    public virtual Tag Tag { get; set; }
}