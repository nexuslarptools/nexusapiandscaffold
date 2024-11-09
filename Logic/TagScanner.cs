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
            var tagslist = new JsonElement();

            if (sheet.TagsField != null)
            {
                sheet.TagsField.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = sheet.TagsField.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                        if (!allowedTags.Contains(Guid.Parse(tag.GetString())))
                            okay = false;
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
            var tagslist = new JsonElement();

            if (sheet.TagsField != null)
            {
                sheet.TagsField.RootElement.TryGetProperty("SeriesTags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = sheet.TagsField.RootElement.GetProperty("SeriesTags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                        if (!allowedTags.Contains(Guid.Parse(tag.GetString())))
                            okay = false;
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
            var tagslist = new JsonElement();

            if (sheet.TagsField != null)
            {
                sheet.TagsField.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined") output.Add(sheet.Guid, tagslist);
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
                tagsout.Add(tagVal);
            }

            return tagsout;
        }

        return null;
    }
}