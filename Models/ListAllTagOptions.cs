namespace NEXUSDataLayerScaffold.Models;

public class ListAllTagOptions
{
    public ListAllTagOptions()
    {
        CharacterLists = new ListCharacterSheets();
        ItemLists = new ListItems();
        SeriesList = new ListSeries();
    }

    public ListCharacterSheets CharacterLists { get; set; }
    public ListItems ItemLists { get; set; }
    public ListSeries SeriesList { get; set; }
}