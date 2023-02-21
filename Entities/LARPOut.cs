using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class LARPOut
{
    public LARPOut()
    {
    }

    public LARPOut(Guid guid, string name, string shortName, string location, bool? isactive)
    {
        Guid = guid;
        Name = name;
        Shortname = shortName;
        Location = location;
        Isactive = isactive;
    }

    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Shortname { get; set; }
    public string Location { get; set; }
    public bool? Isactive { get; set; }
    public List<UserOut> Users { get; set; }
}