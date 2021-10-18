using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class TagTypes
    {
        public TagTypes()
        {
            Tags = new HashSet<Tags>();
        }

        public Guid Guid { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Tags> Tags { get; set; }
    }
}
