using System;

namespace NEXUSDataLayerScaffold.Entities;

public class LARPOut
{
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
}