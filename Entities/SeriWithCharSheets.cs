using NEXUSDataLayerScaffold.Extensions;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities
{
    public class SeriWithCharSheets : Seri
    {
        public List<CharSheetMini> CharSheets { get; set; }
    }
}
