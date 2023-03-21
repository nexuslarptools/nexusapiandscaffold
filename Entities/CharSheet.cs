using System;
using System.Collections.Generic;
using System.Text.Json;
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
    public byte[] imagedata1 { get; set; }
    public byte[] imagedata2 { get; set; }

    public CharacterSheet OutputToCharacterSheet() 
    {
        CharacterSheet Charsheet = new CharacterSheet() {
            Guid = Guid,
            Seriesguid = this.Seriesguid,
            Name = this.Name,
            Img1 = this.Img1,
            Img2 = this.Img2,
            Fields = JsonDocument.Parse(this.Fields.ToString()),
            Isactive = this.Isactive,
            Createdate = this.Createdate,
            CreatedbyuserGuid = this.CreatedbyUserGuid,
            FirstapprovalbyuserGuid = this.FirstapprovalbyUserGuid,
            Firstapprovaldate = this.Firstapprovaldate,
            SecondapprovalbyuserGuid = this.SecondapprovalbyUserGuid,
            Secondapprovaldate = this.Secondapprovaldate,
            Gmnotes = this.Gmnotes,
            Reason4edit = this.Reason4edit
        };

        return Charsheet;
    }

    
}