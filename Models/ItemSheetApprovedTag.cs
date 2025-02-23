using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class ItemSheetApprovedTag
{
    public int Id { get; set; }

    public int ItemsheetapprovedId { get; set; }

    public Guid TagGuid { get; set; }

    public virtual ItemSheetApproved Itemsheetapproved { get; set; }

    public virtual Tag Tag { get; set; }
}
