using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Models;

public class Series
{
    public Guid Guid { get; set; }

    public string Title { get; set; }

    public string Titlejpn { get; set; }

    public JsonDocument Tags { get; set; }

    public bool? Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public DateTime? Deactivedate { get; set; }

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApproveds { get; } =
        new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersions { get; } =
        new List<CharacterSheetVersion>();

    public virtual ICollection<CharacterSheet> CharacterSheets { get; } = new List<CharacterSheet>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApproveds { get; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersions { get; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemSheet> ItemSheets { get; } = new List<ItemSheet>();

    public virtual ICollection<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; } =
        new List<LarpplayerSeriesAllowed>();

    public virtual ICollection<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; } =
        new List<LarpplayerSeriesDisllowed>();

    public virtual ICollection<SeriesTag> SeriesTags { get; } = new List<SeriesTag>();
}