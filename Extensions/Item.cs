using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Extensions;

public class Item
{
    public static IteSheet CreateItem(ItemSheet iSheet)
    {
        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = JObject.Parse(iSheet.Fields.RootElement.ToString());
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<Tags>();

        return newiemSheet;
    }

    public static IteSheet CreateItem(ItemSheetApproved iSheet)
    {
        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = JObject.Parse(iSheet.Fields.RootElement.ToString());
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<Tags>();


        return newiemSheet;
    }

    public static IteSheet CreateItem(ItemSheetVersion iSheet)
    {
        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = JObject.Parse(iSheet.Fields.RootElement.ToString());
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<Tags>();

        return newiemSheet;
    }
}