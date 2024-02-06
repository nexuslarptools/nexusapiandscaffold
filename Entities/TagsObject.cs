using System.Collections.Generic;
using System;

namespace NEXUSDataLayerScaffold.Entities
{
    public class TagsObject
    {
        public List<Guid> MainTags { get; set; }
        public List<Guid> AbilityTags { get; set; }

        public TagsObject() 
        {
            MainTags=new List<Guid>();
            AbilityTags=new List<Guid>();
        }
    }
}
