using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class LARPOut
    {
        public LARPOut()
        {
        }

        public LARPOut(Guid guid, string name, string shortName, string location, bool? isactive)
        {
            this.Guid = guid;
            this.Name = name;
            this.Shortname = shortName;
            this.Location = location;
            this.Isactive = isactive;
        }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Shortname { get; set; }
        public string Location { get; set; }
        public bool? Isactive { get; set; }
        public List<UserOut> Users { get; set; }

    }
}
