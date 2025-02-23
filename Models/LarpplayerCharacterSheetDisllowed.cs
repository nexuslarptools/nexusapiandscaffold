using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class LarpplayerCharacterSheetDisllowed
{
    public Guid Guid { get; set; }

    public Guid? LarpGuid { get; set; }

    public Guid CharactersheetGuid { get; set; }

    public Guid CreatedbyuserGuid { get; set; }

    public bool Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual Larp Larp { get; set; }
}
