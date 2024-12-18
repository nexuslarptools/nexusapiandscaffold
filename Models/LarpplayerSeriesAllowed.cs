﻿using System;

namespace NEXUSDataLayerScaffold.Models;

public class LarpplayerSeriesAllowed
{
    public Guid Guid { get; set; }

    public Guid? LarpGuid { get; set; }

    public Guid SeriesGuid { get; set; }

    public Guid CreatedbyuserGuid { get; set; }

    public bool? Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual Larp Larp { get; set; }

    public virtual Series Series { get; set; }
}