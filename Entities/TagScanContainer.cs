using System;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Entities;

public class TagScanContainer
{
    public TagScanContainer(Guid guid, JsonDocument tagsField)
    {
        Guid = guid;
        TagsField = tagsField;
    }

    public Guid Guid { get; set; }
    public JsonDocument TagsField { get; set; }
}