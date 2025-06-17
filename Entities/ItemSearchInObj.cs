using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class ItemSearchInObj
    {
        public string Name { get; set; }
        public string ItemType { get; set; }
        public List<AttributeSearchTerm> AndAttributeSkillList { get; set; }
        public List<System.Guid> AndTagsList { get; set; }
        public List<SpecialSkillSearchTerm> AndSpecialSkillList { get; set; }
        public List<AttributeSearchTerm> OrAttributeSkillList { get; set; }
        public List<System.Guid> OrTagsList { get; set; }
        public List<SpecialSkillSearchTerm> OrSpecialSkillList { get; set; }

    }

}
