using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class ItemType
{
    public Guid Guid { get; set; }

    public string Type { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<ItemSheetApproved> ItemSheetApproveds { get; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheet> ItemSheets { get; } = new List<ItemSheet>();
}