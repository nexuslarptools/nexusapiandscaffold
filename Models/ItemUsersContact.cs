using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class ItemUsersContact
{
    public Guid Guid { get; set; }

    public Guid ItemsheetGuid { get; set; }

    public Guid UserGuid { get; set; }

    public Guid CreatedbyuserGuid { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createddate { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual User User { get; set; }
}
