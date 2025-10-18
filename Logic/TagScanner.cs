using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Logic;

public class TagScanner
{
    public static List<Guid> ScanTags(List<TagScanContainer> allSheets, List<Guid?> allowedTags)
    {
        var outputter = new List<Guid>();

        foreach (var sheet in allSheets)
        {
            var okay = true;

            if (sheet.TagsField != null)
            {
                if (sheet.TagsField.RootElement.TryGetProperty("Tags", out var tagsElement)
                    && tagsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagsElement.EnumerateArray())
                    {
                        var tagStr = tag.GetString();
                        if (!string.IsNullOrWhiteSpace(tagStr))
                        {
                            if (!allowedTags.Contains(Guid.Parse(tagStr)))
                            {
                                okay = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (okay) outputter.Add(sheet.Guid);
        }

        return outputter;
    }

    public static List<Guid> ScanTagsSeries(List<TagScanContainer> allSheets, List<Guid?> allowedTags)
    {
        var outputter = new List<Guid>();

        foreach (var sheet in allSheets)
        {
            var okay = true;

            if (sheet.TagsField != null)
            {
                if (sheet.TagsField.RootElement.TryGetProperty("SeriesTags", out var tagsElement)
                    && tagsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagsElement.EnumerateArray())
                    {
                        var tagStr = tag.GetString();
                        if (!string.IsNullOrWhiteSpace(tagStr))
                        {
                            if (!allowedTags.Contains(Guid.Parse(tagStr)))
                            {
                                okay = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (okay) outputter.Add(sheet.Guid);
        }

        return outputter;
    }

    public static Dictionary<Guid, JsonElement> getAllTagsLists(List<TagScanContainer> allSheets)
    {
        var output = new Dictionary<Guid, JsonElement>();

        foreach (var sheet in allSheets)
        {
            if (sheet.TagsField != null)
            {
                if (sheet.TagsField.RootElement.TryGetProperty("Tags", out var tagsElement)
                    && tagsElement.ValueKind == JsonValueKind.Array)
                {
                    output.Add(sheet.Guid, tagsElement);
                }
            }
        }

        return output;
    }

    public static List<Tag> ReturnDictElementOrNull(Guid value, Dictionary<Guid, JsonElement> tagDictionary,
        List<Tag> fulltaglist)
    {
        if (tagDictionary.TryGetValue(value, out var tagList))
        {
            var jsontags = JArray.Parse(tagList.ToString());
            var tagsout = new List<Tag>();
            foreach (var tag in jsontags)
            {
                var tagVal = fulltaglist.Where(t => t.Guid == (Guid)tag).FirstOrDefault();
                if (tagVal != null)
                {
                    tagsout.Add(tagVal);
                }
            }

            return tagsout;
        }

        return null;
    }
}
