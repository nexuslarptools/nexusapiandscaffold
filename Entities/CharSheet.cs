using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class CharSheet
{
    public Guid Guid { get; set; }
    public Guid? Seriesguid { get; set; }
    public string SeriesTitle { get; set; }
    public string Name { get; set; }
    public string Img1 { get; set; }
    public string Img2 { get; set; }
    public JObject Fields { get; set; }
    public IteSheet Sheet_Item { get; set; }
    public List<IteSheet> Starting_Items { get; set; }
    public bool? Isactive { get; set; }
    public DateTime Createdate { get; set; }
    public Guid? CreatedbyUserGuid { get; set; }
    public string createdby { get; set; }
    public Guid? FirstapprovalbyUserGuid { get; set; }
    public string Firstapprovalby { get; set; }
    public DateTime? Firstapprovaldate { get; set; }
    public Guid? SecondapprovalbyUserGuid { get; set; }
    public string Secondapprovalby { get; set; }
    public DateTime? Secondapprovaldate { get; set; }
    public string Gmnotes { get; set; }
    public string Reason4edit { get; set; }
    public int Version { get; set; }
    public List<Tags> Tags { get; set; }
}