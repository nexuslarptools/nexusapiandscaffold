using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class CharListOut
    {
        public CharListOut()
        {
            CharList = new List<CharSheet>();
        }

        public List<CharSheet> CharList { get; set; }
        public int fulltotal { get; set; }
    }

}
