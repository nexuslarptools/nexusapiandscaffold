using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Entities
{
    public class IteSheetInput
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid? Seriesguid { get; set; }
        public string Series { get; set; }
        public string Name { get; set; }
        public string Img1 { get; set; }
        public JObject Fields { get; set; }
        public bool? Isactive { get; set; }
        public DateTime Createdate { get; set; }
        public Guid? CreatedbyuserGuid { get; set; }
        public string createdby { get; set; }
        public Guid? FirstapprovalbyuserGuid { get; set; }
        public string Firstapprovalby { get; set; }
        public DateTime? Firstapprovaldate { get; set; }
        public Guid? SecondapprovalbyuserGuid { get; set; }
        public string Secondapprovalby { get; set; }
        public DateTime? Secondapprovaldate { get; set; }
        public string Gmnotes { get; set; }
        public string Reason4edit { get; set; }
        public int? Version { get; set; }
        public List<Guid> Tags { get; set; }
        public Byte[] imagedata { get; set; }
    }
}
