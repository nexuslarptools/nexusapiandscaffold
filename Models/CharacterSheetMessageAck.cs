using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class CharacterSheetMessageAck
{
    public int Id { get; set; }

    public Guid UserGuid { get; set; }

    public int CharactersheetreviewmessagesId { get; set; }

    public bool Isactive { get; set; }

    public DateTime? Seendate { get; set; }

    public virtual CharacterSheetReviewMessage Charactersheetreviewmessages { get; set; }

    public virtual User User { get; set; }
}
