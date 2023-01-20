using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class TagsInput
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public Guid Tagtypeguid { get; set; }
    public bool? Isactive { get; set; }
    public List<Guid> LarptagGuid { get; set; }
}