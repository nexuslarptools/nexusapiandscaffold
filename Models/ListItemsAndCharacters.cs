namespace NEXUSDataLayerScaffold.Models;

public class ListItemsAndCharacters
{
    public ListItemsAndCharacters()
    {
        CharacterLists = new ListCharacterSheets();
        ItemLists = new ListItems();
    }

    public ListCharacterSheets CharacterLists { get; set; }
    public ListItems ItemLists { get; set; }
}