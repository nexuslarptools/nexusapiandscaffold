using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class ItemSheetMessageAck
{
    public int Id { get; set; }

    public Guid UserGuid { get; set; }

    public int ItemsheetreviewmessagesId { get; set; }

    public bool Isactive { get; set; }

    public DateTime? Seendate { get; set; }

    public virtual ItemSheetReviewMessage Itemsheetreviewmessages { get; set; }

    public virtual User User { get; set; }
}
