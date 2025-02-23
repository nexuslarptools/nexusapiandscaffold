using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Models;

public partial class Series
{
    public Guid Guid { get; set; }

    public string Title { get; set; }

    public string Titlejpn { get; set; }

    public JsonDocument Tags { get; set; }

    public bool Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public DateTime? Deactivedate { get; set; }

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApproveds { get; set; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersions { get; set; } = new List<CharacterSheetVersion>();

    public virtual ICollection<CharacterSheet> CharacterSheets { get; set; } = new List<CharacterSheet>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApproveds { get; set; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersions { get; set; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemSheet> ItemSheets { get; set; } = new List<ItemSheet>();

    public virtual ICollection<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; set; } = new List<LarpplayerSeriesAllowed>();

    public virtual ICollection<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; set; } = new List<LarpplayerSeriesDisllowed>();

    public virtual ICollection<SeriesTag> SeriesTags { get; set; } = new List<SeriesTag>();
}
