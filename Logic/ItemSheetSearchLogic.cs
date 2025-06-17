using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Logic;
using System.Collections.Generic;
using System;
using NEXUSDataLayerScaffold.Models;
using NEXUSDataLayerScaffold.Entities;
using System.Linq;

namespace NEXUSDataLayerScaffold.Logic
{
    public class ItemSheetSearchLogic
    {

        public ItemSheetSearchLogic()
        {
            sheets = new List<ItemSheetPullObj>();
            andAttrisheets = new List<Guid>();
            orAttrisheets = new List<Guid>();
            searchInObj = new ItemSearchInObj();
        }

        public List<ItemSheetPullObj> sheets { get; set; }
        public List<Guid> andAttrisheets { get; set; }
        public List<Guid> orAttrisheets { get; set; }
        public ItemSearchInObj searchInObj { get; set; }

        public void DoFullSearch()
        {
            this.andAttrisheets = sheets.Select(g => g.Guid).ToList();

            if (searchInObj.Name != null)
            {
                andAttrisheets = sheets.Where(tc => tc.Name.ToLower().Contains(searchInObj.Name.ToLower())).Select(g => g.Guid).ToList();
            }

            if (searchInObj.ItemType != null && searchInObj.ItemType != "" && searchInObj.ItemType != " ")
            {
                andAttrisheets = sheets.Where(ts =>
                     ts.Fields.RootElement.TryGetProperty("TYPE", out var value))
                .Where(a => a.Fields.RootElement.GetProperty("TYPE").ToString().ToLower() == searchInObj.ItemType.ToLower())
                .Select(g => g.Guid).ToList();
            }

            if (searchInObj.AndAttributeSkillList != null)
            {
                foreach (var obj in searchInObj.AndAttributeSkillList)
                {
                    andAttrisheets = SearchCompareLogic.AttributeCompareMain(
                        sheets.Where(cs => andAttrisheets.Contains(cs.Guid)).ToList(), obj).ToList();
                }
            }

            //And Tags List
            if (searchInObj.AndTagsList != null)
            {
                foreach (var tagGuid in searchInObj.AndTagsList)
                {
                    andAttrisheets = sheets.Where(tc => andAttrisheets.Contains(tc.Guid) &&
                    (tc.MainTags.Contains(tagGuid) || tc.AbilityTags.Contains(tagGuid))
                    ).Select(tc => tc.Guid).ToList();
                }

            }

            this.orAttrisheets = new List<Guid>();


            if ((searchInObj.OrTagsList == null || searchInObj.OrTagsList.Count == 0)
                && (searchInObj.OrAttributeSkillList == null || searchInObj.OrAttributeSkillList.Count == 0)
                 && (searchInObj.OrSpecialSkillList == null || searchInObj.OrSpecialSkillList.Count == 0))
            {
                orAttrisheets = sheets.Select(g => g.Guid).ToList();
            }

            if (searchInObj.OrAttributeSkillList != null)
            {
                foreach (var obj in searchInObj.OrAttributeSkillList)
                {
                    orAttrisheets.AddRange(SearchCompareLogic.AttributeCompareMain(
                        sheets, obj));
                }
            }

            if (searchInObj.OrTagsList != null)
            {
                foreach (var tagGuid in searchInObj.OrTagsList)
                {
                    orAttrisheets.AddRange(sheets.Where(tc =>
                    (tc.MainTags.Contains(tagGuid) || tc.AbilityTags.Contains(tagGuid))
                    ).Select(tc => tc.Guid).ToList());
                }

            }

            //SpecialSkillsAnd 
            if (searchInObj.AndSpecialSkillList != null)
            {
                foreach (var spListitem in searchInObj.AndSpecialSkillList)
                {
                    andAttrisheets = SearchCompareLogic.SpecialSkillCompareMain(
                        sheets.Where(s => andAttrisheets.Contains(s.Guid)).ToList(), spListitem);
                }
            }

            //SpecialSkillsOr 
            if (searchInObj.OrSpecialSkillList != null)
            {
                foreach (var spListitem in searchInObj.OrSpecialSkillList)
                {
                    orAttrisheets.AddRange(SearchCompareLogic.SpecialSkillCompareMain(
                        sheets, spListitem));
                }
            }
        }
    }
}
