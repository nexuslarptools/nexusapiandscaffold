using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Entities
{
    public class ItemSheetPullObj
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public Guid? Seriesguid { get; set; }
        public string Title { get; set; }
        public JsonDocument Fields { get; set; }
        public JsonDocument Fields2ndside { get; set; }
        public List<Guid> MainTags { get; set; }
        public List<Guid> AbilityTags { get; set; }
    }
}

