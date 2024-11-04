using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public class ListCharacterSheets
{
    public ListCharacterSheets()
    {
        ApprovedCharacterSheets = new List<ShortCharacterSheetInfo>();
        UnapprovedCharacterSheets = new List<ShortCharacterSheetInfo>();
    }

    public List<ShortCharacterSheetInfo> ApprovedCharacterSheets { get; set; }
    public List<ShortCharacterSheetInfo> UnapprovedCharacterSheets { get; set; }

    public void AddUnapproved(CharacterSheet cSheet)
    {
        var newinfo = new ShortCharacterSheetInfo();
        newinfo.Guid = cSheet.Guid;
        newinfo.Name = cSheet.Name;
        UnapprovedCharacterSheets.Add(newinfo);
    }

    public void AddApproved(CharacterSheetApproved cSheet)
    {
        var newinfo = new ShortCharacterSheetInfo();
        newinfo.Guid = cSheet.Guid;
        newinfo.Name = cSheet.Name;
        ApprovedCharacterSheets.Add(newinfo);
    }

    public bool ApprovedContainsGuid(Guid guid)
    {
        foreach (var item in ApprovedCharacterSheets)
            if (item.Guid == guid)
                return true;
        return false;
    }

    public bool UnapprovedContainsGuid(Guid guid)
    {
        foreach (var item in UnapprovedCharacterSheets)
            if (item.Guid == guid)
                return true;
        return false;
    }
}

public class ShortCharacterSheetInfo
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
}