using System.Collections.Generic;
using System;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities
{
    public class TagsObject
    {
        public List<Guid> MainTags { get; set; }
        public List<Guid> AbilityTags { get; set; }

        public TagsObject()
        {
            MainTags = new List<Guid>();
            AbilityTags = new List<Guid>();
        }


        public List<ItemSheetTag> OutputItemSheetTags(int sheetID)
        {
            List<ItemSheetTag> newItemSheetTags = new List<ItemSheetTag>();

            foreach (var tagValue in this.MainTags)
            {
                newItemSheetTags.Add(new ItemSheetTag
                {
                    ItemsheetId = sheetID,
                    TagGuid = tagValue
                });
            }

            foreach (var tagValue in this.AbilityTags)
            {
                newItemSheetTags.Add(new ItemSheetTag
                {
                    ItemsheetId = sheetID,
                    TagGuid = tagValue
                });
            }

            return newItemSheetTags;
        }

        public List<ItemSheetApprovedTag> OutputItemSheetApprovedTags(int sheetID)
        {
            List<ItemSheetApprovedTag> newItemSheetTags = new List<ItemSheetApprovedTag>();

            foreach (var tagValue in this.MainTags)
            {
                newItemSheetTags.Add(new ItemSheetApprovedTag
                {
                    ItemsheetapprovedId = sheetID,
                    TagGuid = tagValue
                });
            }

            foreach (var tagValue in this.AbilityTags)
            {
                newItemSheetTags.Add(new ItemSheetApprovedTag
                {
                    ItemsheetapprovedId = sheetID,
                    TagGuid = tagValue
                });
            }

            return newItemSheetTags;
        }
    }
}
