using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class ItemSheetReviewSubscription
{
    public int Id { get; set; }

    public Guid UserGuid { get; set; }

    public Guid ItemsheetGuid { get; set; }

    public DateTime Createdate { get; set; }

    public DateTime? Stopdate { get; set; }

    public virtual User User { get; set; }
}
