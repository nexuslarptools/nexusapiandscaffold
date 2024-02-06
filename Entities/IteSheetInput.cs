using System;
using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

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
    public byte[] imagedata { get; set; }
    public bool Readyforapproval { get; set; }

    public ItemSheet OutputToItemSheet()
    {
        ItemSheet output = new ItemSheet()
        {
            Version = 1,
            Guid = this.Guid,
            Id = this.Id,
            Seriesguid = this.Seriesguid,
            Name = this.Name,
            Img1 = this.Img1,
            Fields = null,
            Isactive = true,
            CreatedbyuserGuid = this.CreatedbyuserGuid,
            FirstapprovalbyuserGuid = this.FirstapprovalbyuserGuid,
            Firstapprovaldate = this.Firstapprovaldate,
            Secondapprovaldate = this.Secondapprovaldate,
            SecondapprovalbyuserGuid = this.SecondapprovalbyuserGuid,
            Gmnotes = this.Gmnotes,
            Reason4edit = this.Reason4edit,
            Readyforapproval = this.Readyforapproval

        };

        if (this.Fields != null)
        {
            output.Fields = JsonDocument.Parse(this.Fields.ToString());
        }

        return output;
    }
}