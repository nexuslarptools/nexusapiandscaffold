using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class TagsOutput
{
    public TagsOutput()
    {
        TagType = string.Empty;
        TagsList = new List<outTag>();
    }

    public string TagType { get; set; }
    public List<outTag> TagsList { get; set; }
}

public class outTag
{
    public outTag(string name, Guid guid, Guid typeGuid, bool? isactive, bool isLocked)
    {
        Name = name;
        Guid = guid;
        Tagtypeguid = typeGuid;
        Isactive = isactive;
        IsLocked = IsLocked;
    }

    public string Name { get; set; }
    public Guid Guid { get; set; }
    public Guid Tagtypeguid { get; set; }
    public bool? Isactive { get; set; }
    public bool IsLocked { get; set; }
    public List<LARPOut> LarpsTagLockedTo { get; set; }
}