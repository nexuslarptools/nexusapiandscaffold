using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class TagScanContainer
    {

        public Guid Guid { get; set; }
        public JsonDocument TagsField { get; set; }

        public  TagScanContainer(Guid guid, JsonDocument tagsField)
        {
            this.Guid = guid;
            this.TagsField = tagsField;
        }

    }
}
