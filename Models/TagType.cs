using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class TagType
{
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Tag> Tags { get; } = new List<Tag>();
}
