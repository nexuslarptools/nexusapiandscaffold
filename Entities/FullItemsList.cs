using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public class FullItemsList
{
    public FullItemsList()
    {
        ApprovedItemsList = new List<ItemsEntry>();
        ItemsList = new List<ItemsEntry>();
    }

    public List<ItemsEntry> ApprovedItemsList { get; set; }
    public List<ItemsEntry> ItemsList { get; set; }
}

public class ItemsEntry
{
    public ItemsEntry(Guid guid, string name)
    {
        this.guid = guid;
        this.name = name;
    }

    public Guid guid { get; set; }
    public string name { get; set; }
}