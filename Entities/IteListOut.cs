using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class IteListOut
{
    public IteListOut()
    {
        IteList = new List<IteSheet>();
    }

    public List<IteSheet> IteList { get; set; }
    public int fulltotal { get; set; }
}