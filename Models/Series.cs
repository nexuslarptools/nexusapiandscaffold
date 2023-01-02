using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class Series
    {
        public Series()
        {
            CharacterSheet = new HashSet<CharacterSheet>();
            CharacterSheetApproved = new HashSet<CharacterSheetApproved>();
            CharacterSheetVersion = new HashSet<CharacterSheetVersion>();
            ItemSheet = new HashSet<ItemSheet>();
            ItemSheetApproved = new HashSet<ItemSheetApproved>();
            ItemSheetVersion = new HashSet<ItemSheetVersion>();
        }

        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Titlejpn { get; set; }
        public JsonDocument Tags { get; set; }
        public bool? Isactive { get; set; }
        public DateTime Createdate { get; set; }
        public DateTime? Deactivedate { get; set; }

        public virtual ICollection<CharacterSheet> CharacterSheet { get; set; }
        public virtual ICollection<CharacterSheetApproved> CharacterSheetApproved { get; set; }
        public virtual ICollection<CharacterSheetVersion> CharacterSheetVersion { get; set; }
        public virtual ICollection<ItemSheet> ItemSheet { get; set; }
        public virtual ICollection<ItemSheetApproved> ItemSheetApproved { get; set; }
        public virtual ICollection<ItemSheetVersion> ItemSheetVersion { get; set; }
    }
}
