using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class SpecialSkillObj
    {
        public Guid Guid { get; set; }
        public Skill Skill { get; set; }
        public int Ord { get; set; }
    }

    public class Skill
    {
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Cost { get; set; }
        public string Uses { get; set; }
        public string Description { get; set; }
        public List<Guid> TagsList { get; set; }
    }
}
