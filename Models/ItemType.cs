using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class ItemType
{
    public Guid Guid { get; set; }

    public string Type { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<ItemSheetApproved> ItemSheetApproveds { get; set; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheet> ItemSheets { get; set; } = new List<ItemSheet>();
}
