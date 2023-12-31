using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Extensions;

public class Character
{
    public static CharSheet CreateCharSheet(CharacterSheet cSheet)
    {
        var newCharSheet = new CharSheet();

        newCharSheet.Guid = cSheet.Guid;
        newCharSheet.Seriesguid = cSheet.Seriesguid;

        newCharSheet.Fields = JObject.Parse(cSheet.Fields.RootElement.ToString());
        newCharSheet.Name = cSheet.Name;
        newCharSheet.Img1 = cSheet.Img1;
        newCharSheet.Img2 = cSheet.Img2;

        newCharSheet.Isactive = cSheet.Isactive;
        newCharSheet.Createdate = cSheet.Createdate;
        newCharSheet.CreatedbyUserGuid = cSheet.CreatedbyuserGuid;
        newCharSheet.FirstapprovalbyUserGuid = cSheet.FirstapprovalbyuserGuid;
        newCharSheet.Firstapprovaldate = cSheet.Firstapprovaldate;
        newCharSheet.SecondapprovalbyUserGuid = cSheet.SecondapprovalbyuserGuid;
        newCharSheet.Secondapprovaldate = cSheet.Secondapprovaldate;
        newCharSheet.Gmnotes = cSheet.Gmnotes;
        newCharSheet.Reason4edit = cSheet.Reason4edit;
        newCharSheet.Version = cSheet.Version;
        newCharSheet.Tags = new List<Tag>();


        return newCharSheet;
    }


    public static CharSheet CreateCharSheet(CharacterSheetApproved cSheet)
    {
        var newCharSheet = new CharSheet();

        newCharSheet.Guid = cSheet.Guid;
        newCharSheet.Seriesguid = cSheet.Seriesguid;
        newCharSheet.Fields = JObject.Parse(cSheet.Fields.RootElement.ToString());
        newCharSheet.Name = cSheet.Name;
        newCharSheet.Img1 = cSheet.Img1;
        newCharSheet.Img2 = cSheet.Img2;


        newCharSheet.Isactive = cSheet.Isactive;
        newCharSheet.Createdate = cSheet.Createdate;
        newCharSheet.CreatedbyUserGuid = cSheet.CreatedbyuserGuid;
        newCharSheet.FirstapprovalbyUserGuid = cSheet.FirstapprovalbyuserGuid;
        newCharSheet.Firstapprovaldate = cSheet.Firstapprovaldate;
        newCharSheet.SecondapprovalbyUserGuid = cSheet.SecondapprovalbyuserGuid;
        newCharSheet.Secondapprovaldate = cSheet.Secondapprovaldate;
        newCharSheet.Gmnotes = cSheet.Gmnotes;
        newCharSheet.Reason4edit = cSheet.Reason4edit;
        newCharSheet.Version = cSheet.Version;
        newCharSheet.Tags = new List<Tag>();


        return newCharSheet;
    }

    public static CharSheet CreateCharSheet(CharacterSheetVersion cSheet)
    {
        var newCharSheet = new CharSheet();

        newCharSheet.Guid = cSheet.Guid;
        newCharSheet.Seriesguid = cSheet.Seriesguid;
        newCharSheet.Fields = JObject.Parse(cSheet.Fields.RootElement.ToString());
        newCharSheet.Name = cSheet.Name;
        newCharSheet.Img1 = cSheet.Img1;
        newCharSheet.Img2 = cSheet.Img2;

        newCharSheet.Isactive = cSheet.Isactive;
        newCharSheet.Createdate = cSheet.Createdate;
        newCharSheet.CreatedbyUserGuid = cSheet.CreatedbyuserGuid;
        newCharSheet.FirstapprovalbyUserGuid = cSheet.FirstapprovalbyuserGuid;
        newCharSheet.Firstapprovaldate = cSheet.Firstapprovaldate;
        newCharSheet.SecondapprovalbyUserGuid = cSheet.SecondapprovalbyuserGuid;
        newCharSheet.Secondapprovaldate = cSheet.Secondapprovaldate;
        newCharSheet.Gmnotes = cSheet.Gmnotes;
        newCharSheet.Reason4edit = cSheet.Reason4edit;
        newCharSheet.Version = cSheet.Version;
        newCharSheet.Tags = new List<Tag>();


        return newCharSheet;
    }
}