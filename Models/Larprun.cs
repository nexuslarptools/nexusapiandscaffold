using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class Larprun
{
    public Guid Guid { get; set; }

    public Guid LarpGuid { get; set; }

    public string Runname { get; set; }

    public DateTime Preregstartdate { get; set; }

    public DateTime Preregenddate { get; set; }

    public DateTime Larprunstartdate { get; set; }

    public DateTime Larprunenddate { get; set; }

    public Guid CreatedbyuserGuid { get; set; }

    public bool? Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual Larp Larp { get; set; }

    public virtual ICollection<LarprunPreReg> LarprunPreRegs { get; } = new List<LarprunPreReg>();
}