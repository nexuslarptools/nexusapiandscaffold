using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class CharListWithItemList
    {
        public List<CharSheet> Characters { get; set; }
        public List<IteSheet> Items { get; set; }

        public CharListWithItemList() 
        {
            Characters = new List<CharSheet>();
            Items = new List<IteSheet>();
        }

        public CharListWithItemList(List<CharSheet> inputchars) 
        {
            Characters = new List<CharSheet>();
            Items = new List<IteSheet>();

            foreach (var charater in inputchars)
            {
                Items.Add(charater.Sheet_Item);
                foreach(var item in charater.Starting_Items)
                {
                    Items.Add(item);
                }

                charater.Sheet_Item = null;
                charater.Starting_Items = null;
            }
        }
    }
}
