﻿using System;

namespace NEXUSDataLayerScaffold.Models;

public class SheetUsersContact
{
    public Guid Guid { get; set; }

    public Guid CharactersheetGuid { get; set; }

    public Guid UserGuid { get; set; }

    public Guid CreatedbyuserGuid { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createddate { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual User User { get; set; }
}