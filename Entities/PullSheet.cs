using System;

namespace NEXUSDataLayerScaffold.Entities
{
    public class PullSheet
    {
        public Guid Guid { get; set; }
        public Guid Seriesguid { get; set; }
        public string Name { get; set; }
        public DateTime Createdate { get; set; }
        public Guid CreatedbyuserGuid { get; set; }
        public Guid? FirstapprovalbyuserGuid { get; set; }
        public DateTime? Firstapprovaldate { get; set; }
        public Guid? SecondapprovalbyuserGuid { get; set; }
        public DateTime? Secondapprovaldate { get; set; }
        public Guid? EditbyUserGuid { get; set; }
        public string Taglists { get; set; }
        public bool Readyforapproval { get; set; }
    }
}
