using System;

namespace NEXUSDataLayerScaffold.Models;

public class LarpplayerTagDisllowed
{
    public Guid Guid { get; set; }

    public Guid LarpGuid { get; set; }

    public Guid TagGuid { get; set; }

    public Guid CreatedbyuserGuid { get; set; }

    public bool? Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual Larp Larp { get; set; }

    public virtual Tag Tag { get; set; }
}