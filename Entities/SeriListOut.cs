using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class SeriListOut
    {
        public SeriListOut()
        {
            SeriList = new List<Seri>();
        }

        public List<Seri> SeriList { get; set; }
        public int fulltotal { get; set; }
    }
}
