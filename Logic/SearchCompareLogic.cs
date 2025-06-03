using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Logic
{
    public class SearchCompareLogic
    {
        public static List<Guid> AttributeCompareMain(List<CharSheetPullObj> sheets, AttributeSearchTerm term)
        {
            try
            {
                //Find attribute name and whittle down sheets to only those with the attribute:
                if (term.Attribute != null)
                {
                    var subsheets = sheets.Where(ts =>
                      ts.Fields.RootElement.TryGetProperty(term.Attribute, out var value))
                      .Select(tc => new JsonFieldRef
                      {
                          Guid = tc.Guid,
                          FieldValue = tc.Fields.RootElement.GetProperty(term.Attribute).ToString()
                      }).ToList();

                    return AttributeCompareSwitch(subsheets, term).Select(acs => acs.Guid).ToList();
                     
                }
                return new List<Guid>();
            } 
            catch(Exception ex)
            {
                throw;
            }
        }

        public static List<Guid> SpecialSkillCompareMain(List<CharSheetPullObj> sheets, SpecialSkillSearchTerm term)
        {
            try
            {
                var output = sheets.Select(s => s.Guid).ToList();
                //Extract Special Skills


                var SpecialSkills = sheets.Where(ts =>
                 ts.Fields.RootElement.TryGetProperty("Special_Skills", out var value)
                 && ts.Fields.RootElement.GetProperty("Special_Skills").ValueKind == JsonValueKind.Array
                 ).Select(s => new
                 {
                     s.Guid,
                     Skills = s.Fields.RootElement.GetProperty("Special_Skills").EnumerateObject()
                 }).ToList().SelectMany(sk => sk.Skills.Select(
                 (ski, i) => new SpecialSkillObj()
                 {
                   Guid = sk.Guid,
                   Skill = JsonConvert.DeserializeObject<Skill>(ski.ToString()),
                   Ord = i
                 }));

                /*              var SpecialSkills = sheets.Where(ts =>
                                   ts.Fields.RootElement.TryGetProperty("Special_Skills", out var value)
                                   && ts.Fields.RootElement.GetProperty("Special_Skills").ValueKind == JsonValueKind.Array
                                   )
                                  .SelectMany(s => s.Fields.RootElement.GetProperty("Special_Skills")
                                  .EnumerateArray(),
                                  (s, skill) => new SpecialSkillObj(){
                                      Guid = s.Guid,
                                      Skill = JsonConvert.DeserializeObject<Skill>(skill.ToString()),
                                      Ord = i
                                  }
                                  ).ToList();*/

                //Perform compare for all extant subqueries.

                if (term.Name != null)
                {
                    var result = AttributeCompareSwitch(SpecialSkills
                        .Where(sp => output.Contains(sp.Guid))
                        .Select(
                        sp => new JsonFieldRef()
                        {
                            Guid = sp.Guid,
                            FieldValue = sp.Skill.Name,
                            Ord = sp.Ord
                        }).ToList(), term.Name);


                    output = output.Where(o => result.Any(r => r.Guid == o)).ToList();
                }

                if (term.Rank != null)
                {
                    var result = AttributeCompareSwitch(SpecialSkills
                   .Where(sp => output.Contains(sp.Guid))
                    .Select(
                     sp => new JsonFieldRef()
                     {
                         Guid = sp.Guid,
                         FieldValue = sp.Skill.Rank,
                         Ord = sp.Ord
                     }).ToList(), term.Rank);

                    output = output.Where(o => result.Any(r => r.Guid == o)).ToList();
                }

                if (term.Cost != null)
                {
                    var result = AttributeCompareSwitch(SpecialSkills
                   .Where(sp => output.Contains(sp.Guid))
                    .Select(
                     sp => new JsonFieldRef()
                     {
                         Guid = sp.Guid,
                         FieldValue = sp.Skill.Cost,
                         Ord = sp.Ord
                     }).ToList(), term.Cost);

                    output = output.Where(o => result.Any(r => r.Guid == o)).ToList();
                }

                if (term.Uses != null)
                {
                    var result = AttributeCompareSwitch(SpecialSkills
                   .Where(sp => output.Contains(sp.Guid))
                    .Select(
                     sp => new JsonFieldRef()
                     {
                         Guid = sp.Guid,
                         FieldValue = sp.Skill.Uses,
                         Ord = sp.Ord
                     }).ToList(), term.Uses);

                    output = output.Where(o => result.Any(r => r.Guid == o)).ToList();
                }

                if (term.Description != null)
                {
                    var fullOut = new List<List<JsonFieldRef>>();
                    var result = new List<JsonFieldRef>();

                    foreach (var item in term.Description)
                    {
                        fullOut.Add(AttributeCompareSwitch(SpecialSkills
                       .Where(sp => output.Contains(sp.Guid))
                        .Select(
                         sp => new JsonFieldRef()
                         {
                             Guid = sp.Guid,
                             FieldValue = sp.Skill.Description,
                             Ord = sp.Ord
                         }).ToList(), item));
                    }

                    for(int i = 0; i < fullOut.Count; i++)
                    {
                        if (i == 0)
                        {
                            result = fullOut[i];
                        }
                        else
                        {
                            result = result.Where(r => fullOut[i].Any(fo => fo.Guid == r.Guid
                            && fo.Ord == r.Ord)).ToList();
                        }
                    }

                    output = output.Where(o => result.Any(r => r.Guid == o)).ToList();
                }

                //return results. 
                return output;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static List<JsonFieldRef> AttributeCompareSwitch(List<JsonFieldRef> refs, AttributeSearchTerm term)
        {

            //Find compare type and compare then return results
            switch (term.CompareType)
            {
                case "Equals":
                    return AttributeCompare(refs, term.Value, "=");
                case "LessThan":
                    return AttributeCompare(refs, term.Value, "<");
                case "GreaterThan":
                    return AttributeCompare(refs, term.Value, ">");
                case "LessThanEqual":
                    return AttributeCompare(refs, term.Value, "<=");
                case "GreaterThanEqual":
                    return AttributeCompare(refs, term.Value, ">=");
                case "ExactEquals":
                    return AttributeCompareExactEquals(refs, term.Value);
                case "Contains":
                    return AttributeCompareContains(refs, term.Value);
                case "StartsWith":
                    return AttributeCompareStartsWith(refs, term.Value);
                case "EndsWith":
                    return AttributeCompareEndsWith(refs, term.Value);
                default:
                    return new List<JsonFieldRef>();

            }
        }

        public static List<JsonFieldRef> AttributeCompareExactEquals (List<JsonFieldRef> sheetrefs, string value)
        {
            return sheetrefs.Where(sr => sr.FieldValue != null && sr.FieldValue.ToLower() == value.ToLower()).ToList();
        }

        public static List<JsonFieldRef> AttributeCompareContains(List<JsonFieldRef> sheetrefs, string value)
        {
            return sheetrefs.Where(sr => sr.FieldValue != null && sr.FieldValue.ToLower().Contains(value.ToLower())).ToList();
        }

        public static List<JsonFieldRef> AttributeCompareStartsWith(List<JsonFieldRef> sheetrefs, string value)
        {
            return sheetrefs.Where(sr => sr.FieldValue != null && sr.FieldValue.ToLower().StartsWith(value.ToLower())).ToList();
        }
        public static List<JsonFieldRef> AttributeCompareEndsWith(List<JsonFieldRef> sheetrefs, string value)
        {
            return sheetrefs.Where(sr => sr.FieldValue != null && sr.FieldValue.ToLower().EndsWith(value.ToLower())).ToList();
        }

        public static List<JsonFieldRef> AttributeCompare(List<JsonFieldRef> sheetrefs, string value, string comparetype)
        {
            try
            {
                return sheetrefs.Where(sr => sr.FieldValue != null &&
                     StringLogic.CompareNumericsWithSlashes(sr.FieldValue, value, comparetype)).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
