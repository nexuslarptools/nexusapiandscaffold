using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;
using System.Linq;

namespace NEXUSDataLayerScaffold.Entities;

public class IteSheet
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
    public List<Tag> Tags { get; set; }
    public byte[] imagedata { get; set; }
    public Guid? EditbyUserGuid { get; set; }
    public string EditbyUser { get; set; }

    public IteSheet()
    {
    }

    public IteSheet(ItemSheet sheet, NexusLarpLocalContext _context)
    {
        Id = sheet.Id;
        Guid = sheet.Guid;
        Name = sheet.Name;
        Img1 = sheet.Img1;
        Seriesguid = sheet.Seriesguid;
        Createdate = sheet.Createdate;
        CreatedbyuserGuid = sheet.CreatedbyuserGuid;
        FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid;
        SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid;
        EditbyUserGuid = sheet.EditbyUserGuid;
        Version = sheet.Version;
        Fields = JObject.Parse(sheet.Fields.RootElement.ToString());
        Tags = new List<Tag>();

        if (this.Img1 != null)
        {
            if (System.IO.File.Exists(@"./images/items/UnApproved/" + sheet.Img1))
            {
                this.imagedata =
                    System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);
            }
        }

        if (this.CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == this.CreatedbyuserGuid)
                .FirstOrDefault();
            this.createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.createdby = creUser.Firstname;
            }
        }

        if (this.FirstapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == this.FirstapprovalbyuserGuid)
                .FirstOrDefault();
            this.Firstapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.Firstapprovalby = creUser.Firstname;
            }
        }

        if (this.SecondapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == this.SecondapprovalbyuserGuid)
                .FirstOrDefault();
            this.Secondapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.Secondapprovalby = creUser.Firstname;
            }
        }

        if (this.EditbyUserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == this.EditbyUserGuid)
                .FirstOrDefault();
            this.EditbyUser = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.EditbyUser = creUser.Firstname;
            }
        }

        JsonElement tagslist = new JsonElement();

        sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

        if (tagslist.ValueKind.ToString() != "Undefined")
        {
            var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

            foreach (var tag in TestJsonFeilds)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                    .FirstOrDefault();
                this.Tags.Add(fullTag);

            }
        }

    }

    public IteSheet(ItemSheetApproved sheet, NexusLarpLocalContext _context)
    {
        Id = sheet.Id;
        Guid = sheet.Guid;
        Name = sheet.Name;
        Img1 = sheet.Img1;
        Seriesguid = sheet.Seriesguid;
        Createdate = sheet.Createdate;
        CreatedbyuserGuid = sheet.CreatedbyuserGuid;
        FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid;
        SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid;
        EditbyUserGuid = sheet.EditbyUserGuid;
        Version = sheet.Version;
        Fields = JObject.Parse(sheet.Fields.RootElement.ToString());
        Tags = new List<Tag>();

        if (this.Img1 != null)
        {
            if (System.IO.File.Exists(@"./images/items/UnApproved/" + sheet.Img1))
            {
                this.imagedata =
                    System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);
            }
        }

        if (this.CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == this.CreatedbyuserGuid)
                .FirstOrDefault();
            this.createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.createdby = creUser.Firstname;
            }
        }

        if (this.FirstapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == this.FirstapprovalbyuserGuid)
                .FirstOrDefault();
            this.Firstapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.Firstapprovalby = creUser.Firstname;
            }
        }

        if (this.SecondapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == this.SecondapprovalbyuserGuid)
                .FirstOrDefault();
            this.Secondapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.Secondapprovalby = creUser.Firstname;
            }
        }

        if (this.EditbyUserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == this.EditbyUserGuid)
                .FirstOrDefault();
            this.EditbyUser = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
            {
                this.EditbyUser = creUser.Firstname;
            }
        }

        JsonElement tagslist = new JsonElement();

        sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

        if (tagslist.ValueKind.ToString() != "Undefined")
        {
            var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

            foreach (var tag in TestJsonFeilds)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                    .FirstOrDefault();
                this.Tags.Add(fullTag);

            }
        }

    }
}