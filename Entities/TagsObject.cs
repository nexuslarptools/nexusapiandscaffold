using System;
using System.Collections.Generic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class TagsObject
{
    public TagsObject()
    {
        MainTags = new List<Guid>();
        AbilityTags = new List<Guid>();
    }

    public List<Guid> MainTags { get; set; }
    public List<Guid> AbilityTags { get; set; }


    public List<ItemSheetTag> OutputItemSheetTags(int sheetID)
    {
        var newItemSheetTags = new List<ItemSheetTag>();

        foreach (var tagValue in MainTags)
            newItemSheetTags.Add(new ItemSheetTag
            {
                ItemsheetId = sheetID,
                TagGuid = tagValue
            });

        foreach (var tagValue in AbilityTags)
            newItemSheetTags.Add(new ItemSheetTag
            {
                ItemsheetId = sheetID,
                TagGuid = tagValue
            });

        return newItemSheetTags;
    }

    public List<ItemSheetApprovedTag> OutputItemSheetApprovedTags(int sheetID)
    {
        var newItemSheetTags = new List<ItemSheetApprovedTag>();

        foreach (var tagValue in MainTags)
            newItemSheetTags.Add(new ItemSheetApprovedTag
            {
                ItemsheetapprovedId = sheetID,
                TagGuid = tagValue
            });

        foreach (var tagValue in AbilityTags)
            newItemSheetTags.Add(new ItemSheetApprovedTag
            {
                ItemsheetapprovedId = sheetID,
                TagGuid = tagValue
            });

        return newItemSheetTags;
    }
}