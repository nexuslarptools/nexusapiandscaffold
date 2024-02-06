using NEXUSDataLayerScaffold.Models;
using System;

namespace NEXUSDataLayerScaffold.Entities
{
    public class TagOut
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public Guid Tagtypeguid { get; set; }

        public string Tagtype { get; set; }

        public TagOut(Tag outputTag)
        {
            Guid = outputTag.Guid;
            Name = outputTag.Name;
            Tagtypeguid= outputTag.Tagtypeguid;
            Tagtype = null;
        }
    }

}
