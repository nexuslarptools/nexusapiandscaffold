using NEXUSDataLayerScaffold.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Logic
{
    public class TagScanner
    {
        public static List<Guid> ScanTags(List<TagScanContainer> allSheets,  List<Guid?> allowedTags)
        {
            var  outputter = new List<Guid>();

            foreach (var sheet in allSheets)
            {
                bool okay = true;
                JsonElement tagslist = new JsonElement();

                sheet.TagsField.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = sheet.TagsField.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        if (!allowedTags.Contains(Guid.Parse(tag.GetString())))
                        {
                            okay = false;
                        }
                    }
                }

                if (okay)
                {
                    outputter.Add(sheet.Guid);
                }
            }
            return outputter;
        }

        public static List<Guid> ScanTagsSeries(List<TagScanContainer> allSheets, List<Guid?> allowedTags)
        {
            var outputter = new List<Guid>();

            foreach (var sheet in allSheets)
            {
                bool okay = true;
                JsonElement tagslist = new JsonElement();

                if (sheet.TagsField != null)
                {
                    sheet.TagsField.RootElement.TryGetProperty("SeriesTags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = sheet.TagsField.RootElement.GetProperty("SeriesTags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            if (!allowedTags.Contains(Guid.Parse(tag.GetString())))
                            {
                                okay = false;
                            }
                        }
                    }
                }

                if (okay)
                {
                    outputter.Add(sheet.Guid);
                }
            }
            return outputter;
        }
    }
}
