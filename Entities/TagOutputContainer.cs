using System;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Entities;

public class TagOutputContainer
{
    public TagOutputContainer(Guid guid, JsonElement tagsField)
    {
        Guid = guid;
        TagsField = tagsField;
    }

    public Guid Guid { get; set; }
    public JsonElement TagsField { get; set; }
}