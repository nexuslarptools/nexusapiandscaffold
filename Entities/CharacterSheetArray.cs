using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class CharacterSheetArray
{
    public CharacterSheetArray()
    {
        Characters = new List<CharSheet>();
        Items = new List<IteSheet>();
    }

    public List<CharSheet> Characters { get; set; }
    public List<IteSheet> Items { get; set; }
}