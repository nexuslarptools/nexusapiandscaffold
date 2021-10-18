using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class Larps
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Shortname { get; set; }
        public string Location { get; set; }
        public bool? Isactive { get; set; }
    }
}
