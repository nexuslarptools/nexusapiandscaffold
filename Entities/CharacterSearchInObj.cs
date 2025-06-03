using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class CharacterSearchInObj
    {
        public string Name { get; set; }
        public string AlternateName { get; set; }
        public List<AttributeSearchTerm> AndAttributeSkillList { get; set; }
        public List<System.Guid> AndTagsList { get; set; }
        public List<SpecialSkillSearchTerm> AndSpecialSkillList { get; set; }
        public List<AttributeSearchTerm> OrAttributeSkillList { get; set; }
        public List<System.Guid> OrTagsList { get; set; }
        public List<SpecialSkillSearchTerm> OrSpecialSkillList { get; set; }

    }

    public class SpecialSkillSearchTerm
    {
        public AttributeSearchTerm Name { get; set; }
        public AttributeSearchTerm Rank { get; set; }
        public AttributeSearchTerm Cost { get; set; }
        public AttributeSearchTerm Uses { get; set; }
        public List<AttributeSearchTerm> Description { get; set; }

    }

    public class AttributeSearchTerm
    {
        public string Attribute { get; set; }
        public string CompareType { get; set; }
        public string Value { get; set; }
    }

}
