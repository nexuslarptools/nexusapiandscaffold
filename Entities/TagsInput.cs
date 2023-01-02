using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class TagsInput
    {

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public Guid Tagtypeguid { get; set; }
        public bool? Isactive { get; set; }
        public List<Guid> LarptagGuid { get; set; }

    }
}
