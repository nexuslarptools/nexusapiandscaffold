namespace NEXUSDataLayerScaffold.Models
{
    public class ListItemsAndCharacters
    {
        public ListCharacterSheets CharacterLists { get; set; }
        public ListItems ItemLists { get; set; }

        public ListItemsAndCharacters() 
        {
            CharacterLists = new ListCharacterSheets();
            ItemLists = new ListItems();
        }
    }
}
