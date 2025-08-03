using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class ShipCrewList
{
    public Guid Guid { get; set; }

    public string Position { get; set; }

    public string Details { get; set; }

    public int Ord { get; set; }

    public bool Isactive { get; set; }

    public DateTime Createdate { get; set; }
}
