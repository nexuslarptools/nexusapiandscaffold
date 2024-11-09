using System;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class TagOut
{
    public TagOut(Tag outputTag)
    {
        Guid = outputTag.Guid;
        Name = outputTag.Name;
        Tagtypeguid = outputTag.Tagtypeguid;
        Tagtype = null;

        if (outputTag.Tagtype != null) Tagtype = outputTag.Tagtype.Name;
    }

    public Guid Guid { get; set; }

    public string Name { get; set; }

    public Guid Tagtypeguid { get; set; }

    public string Tagtype { get; set; }
}