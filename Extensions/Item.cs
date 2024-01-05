using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Extensions;

public class Item
{
    public static IteSheet CreateItem(ItemSheet iSheet)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(iSheet.Fields.RootElement.ToString()));

        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = FeildsWInit;
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<Tag>();

        var folderName = Path.Combine("images", "items", "UnApproved");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            

        if (iSheet.Img1 != null) {
            if (System.IO.File.Exists(pathToSave + iSheet.Img1)) {
                newiemSheet.imagedata =
                    System.IO.File.ReadAllBytes(pathToSave + iSheet.Img1);
            }
        }

        return newiemSheet;
    }

    public static IteSheet CreateItem(ItemSheetApproved iSheet) {

        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(iSheet.Fields.RootElement.ToString()));

        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = FeildsWInit;
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<Tag>();

        var folderName = Path.Combine("images", "items", "Approved");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);


        if (iSheet.Img1 != null) {
            if (System.IO.File.Exists(pathToSave + "\\" + iSheet.Img1)) {
                newiemSheet.imagedata =
                    System.IO.File.ReadAllBytes(pathToSave + "\\" + iSheet.Img1);
            }
        }

        return newiemSheet;
    }

    public static IteSheet CreateItem(ItemSheetVersion iSheet)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(iSheet.Fields.RootElement.ToString()));

        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = FeildsWInit;
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<Tag>();

        return newiemSheet;
    }
}