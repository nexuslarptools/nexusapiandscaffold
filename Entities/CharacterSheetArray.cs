using NEXUSDataLayerScaffold.Models;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class CharacterSheetArray
    {
        public List<CharSheet> Characters {get; set; }
        public List<IteSheet> Items { get; set; }

        public CharacterSheetArray()
        {
            Characters = new List<CharSheet>();
            Items = new List<IteSheet>();
        }
    }
}
