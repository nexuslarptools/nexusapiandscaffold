using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Entities;

public class TagScanContainer
{
    public TagScanContainer(Guid guid, ICollection<ItemSheetTag> tagsList)
    {
        Tags = new List<Guid>();
        Guid = guid;
        foreach (var tag in tagsList)
        {
            Tags.Add(tag.TagGuid);
        }

    }

    public TagScanContainer(Guid guid, ICollection<SeriesTag> tagsList)
    {
        Tags = new List<Guid>();
        Guid = guid;
        foreach (var tag in tagsList)
        {
            Tags.Add(tag.TagGuid);
        }

    }

    public TagScanContainer(Guid guid, ICollection<ItemSheetApprovedTag> tagsList)
    {
        Tags = new List<Guid>();
        Guid = guid;
        foreach (var tag in tagsList)
        {
            Tags.Add(tag.TagGuid);
        }

    }

    public TagScanContainer(Guid guid, ICollection<CharacterSheetTag> tagsList)
    {
        Tags = new List<Guid>();
        Guid = guid;
        foreach (var tag in tagsList)
        {
            Tags.Add(tag.TagGuid);
        }

    }

    public TagScanContainer(Guid guid, ICollection<CharacterSheetApprovedTag> tagsList)
    {
        Tags = new List<Guid>();
        Guid = guid;
        foreach (var tag in tagsList)
        {
            Tags.Add(tag.TagGuid);
        }

    }

    public Guid Guid { get; set; }
    public List<Guid> Tags { get; set; }
    public JsonDocument TagsField;
}