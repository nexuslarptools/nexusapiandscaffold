using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class ItemSheetApproved
    {
        public int Id { get; set; }
        public int ItemsheetId { get; set; }
        public Guid Guid { get; set; }
        public Guid? Seriesguid { get; set; }
        public string Name { get; set; }
        public string Img1 { get; set; }
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
        public int? Version { get; set; }

        public virtual Users CreatedbyuserGu { get; set; }
        public virtual Users FirstapprovalbyuserGu { get; set; }
        public virtual Users SecondapprovalbyuserGu { get; set; }
        public virtual Series Seriesgu { get; set; }
    }
}
