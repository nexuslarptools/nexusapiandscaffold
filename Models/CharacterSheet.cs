using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class CharacterSheet
    {
        public CharacterSheet()
        {
            CharacterSheetApproved = new HashSet<CharacterSheetApproved>();
            CharacterSheetVersion = new HashSet<CharacterSheetVersion>();
        }

        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid? Seriesguid { get; set; }
        public string Name { get; set; }
        public string Img1 { get; set; }
        public string Img2 { get; set; }
        public JsonDocument Fields { get; set; }
        public bool? Isactive { get; set; }
        public DateTime Createdate { get; set; }
        public Guid? CreatedbyuserGuid { get; set; }
        public Guid? FirstapprovalbyuserGuid { get; set; }
        public DateTime? Firstapprovaldate { get; set; }
        public Guid? SecondapprovalbyuserGuid { get; set; }
        public DateTime? Secondapprovaldate { get; set; }
        public string Gmnotes { get; set; }
        public string Reason4edit { get; set; }
        public int Version { get; set; }

        public virtual Users CreatedbyuserGu { get; set; }
        public virtual Users FirstapprovalbyuserGu { get; set; }
        public virtual Users SecondapprovalbyuserGu { get; set; }
        public virtual Series Seriesgu { get; set; }
        public virtual ICollection<CharacterSheetApproved> CharacterSheetApproved { get; set; }
        public virtual ICollection<CharacterSheetVersion> CharacterSheetVersion { get; set; }
    }
}
