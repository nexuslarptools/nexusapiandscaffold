using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class LarpplayerSeriesDisllowed
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
