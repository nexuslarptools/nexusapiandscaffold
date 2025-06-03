using Newtonsoft.Json.Linq;
using System;

namespace NEXUSDataLayerScaffold.Entities
{
    public class JsonFieldRef
    {
        public Guid Guid { get; set; }
        public string FieldValue { get; set; }
        public int Ord { get; set; }

        public JsonFieldRef()
        {
            FieldValue = "";
            Guid = Guid.Empty;
            Ord = 0;
        }
    }
}
