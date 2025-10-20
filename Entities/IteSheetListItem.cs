using NEXUSDataLayerScaffold.Models;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace NEXUSDataLayerScaffold.Entities;

public class IteSheetListItem
{
    public IteSheetListItem(ItemSheetDO charSheet)
    {
        var tagslist = new JsonElement();
        tags = new List<TagOut>();


        foreach (var tag in charSheet.TagList)
            if (tag.Tagtypeguid == Guid.Parse("26cd7510-9401-11ea-899a-4fd87913c65d") ||
                tag.Tagtypeguid == Guid.Parse("18ac6dfa-86e9-11ed-956c-a37a96501122"))
                tags.Add(new TagOut(tag));


        guid = charSheet.Sheet.Guid;
        name = charSheet.Sheet.Name;
        seriesguid = charSheet.Series.Guid;
        title = charSheet.Series.Title;
        createdbyuserGuid = charSheet.Createdbyuser.Guid;
        createdByUser = charSheet.Createdbyuser.Preferredname;
        editbyUserGuid = charSheet.Sheet.EditbyUserGuid;
        firstapprovalbyuserGuid = charSheet.Sheet.FirstapprovalbyuserGuid == null ? null : charSheet.Sheet.FirstapprovalbyuserGuid;
        firstApprovalUser = charSheet.Firstapprovalbyuser == null ? null : charSheet.Firstapprovalbyuser.Preferredname;
        secondapprovalbyuserGuid = charSheet.Sheet.SecondapprovalbyuserGuid == null ? null : charSheet.Sheet.SecondapprovalbyuserGuid;
        ;
        secondApprovalUser =
            charSheet.Secondapprovalbyuser == null ? null : charSheet.Secondapprovalbyuser.Preferredname;
        editbyUser = charSheet.EditbyUser == null ? null : charSheet.EditbyUser.Preferredname;
        hasreview = charSheet.ListMessages.Count > 0 ? true : false;
        readyforapproval = charSheet.Sheet.Readyforapproval;
        isactive = charSheet.Sheet.Isactive;
    }

    public Guid guid { get; set; }
    public string name { get; set; }
    public Guid seriesguid { get; set; }
    public string title { get; set; }
    public Guid createdbyuserGuid { get; set; }
    public string createdByUser { get; set; }
    public Guid? firstapprovalbyuserGuid { get; set; }
    public string firstApprovalUser { get; set; }
    public Guid? secondapprovalbyuserGuid { get; set; }
    public string secondApprovalUser { get; set; }
    public string editbyUser { get; set; }
    public Guid? editbyUserGuid { get; set; }
    public bool readyforapproval { get; set; }
    public List<TagOut> tags { get; set; }
    public bool hasreview { get; set; }
    public bool isactive { get; set; }
    public bool wasapproved { get; set; }

    public bool IsNullOrEmpty(Guid? input)
    {
        if (input == null || input == Guid.Empty) return true;

        return false;
    }

}