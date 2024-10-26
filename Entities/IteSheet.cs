using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;
using System.Linq;
using NEXUSDataLayerScaffold.Logic;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;

namespace NEXUSDataLayerScaffold.Entities;

public class IteSheet
{
    private object sheet;
    private NexusLarpLocalContext context;

    public int Id { get; set; }
    public Guid Guid { get; set; }
    public Guid? Seriesguid { get; set; }
    public string Series { get; set; }
    public Guid? ItemTypeGuid { get; set; }
    public string Type { get; set; }
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
    public List<TagOut> Tags { get; set; }
    public byte[] imagedata { get; set; }
    public Guid? EditbyUserGuid { get; set; }
    public string EditbyUser { get; set; }
    public bool readyforapproval { get; set; }
    public bool hasreview { get; set; }
    public bool Isdoubleside { get; set; }
    public bool Isfrontonly { get; set; }
    public bool Isbackonly { get; set; }
    public Backside Back { get; set; }

    public List<ReviewMessage> ReviewMessages { get; set; }


    public IteSheet()
    {
    }

    public IteSheet(ItemSheet sheet, NexusLarpLocalContext _context)
    {
        JsonElement tagslist = new JsonElement();
        Tags = new List<TagOut>();

        if (sheet.ItemSheetTags != null)
        {
            foreach (var tag in sheet.ItemSheetTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag.TagGuid)
                    .Include("Tagtype").FirstOrDefault();
                if (fullTag != null)
                {
                    this.Tags.Add(new TagOut(fullTag));
                }
            }
        }

        if (sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.ItemtypeGuid).FirstOrDefault();
            this.Type = thistype.Type;
            this.ItemTypeGuid = (Guid)sheet.ItemtypeGuid;
        }
        else
        {
            Type = "Generic";
        }

        if (sheet.Fields != null)
        {
            Fields = JObject.Parse(sheet.Fields.RootElement.ToString());
            if (this.Tags.Count == 0)
            {
                var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(sheet.Fields.RootElement.ToString()));
                Fields = FeildsWInit;
            }
        }

        Id = sheet.Id;
        Guid = sheet.Guid;
        Isactive = sheet.Isactive;
        Name = sheet.Name;
        Img1 = sheet.Img1;
        Seriesguid = sheet.Seriesguid;
        Createdate = sheet.Createdate;
        CreatedbyuserGuid = sheet.CreatedbyuserGuid;
        FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid;
        SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid;
        EditbyUserGuid = sheet.EditbyUserGuid;
        Version = sheet.Version;
        readyforapproval = sheet.Readyforapproval;
        Gmnotes = sheet.Gmnotes;
        Isdoubleside = (bool)sheet.Isdoubleside;
        Back = new Backside(sheet);

        if (this.Type != null)
        {
            this.Back.Type = this.Type;
        }


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

        if (this.Seriesguid != null)
        {
            var curseries = _context.Series
                .Where(u => u.Guid == this.Seriesguid)
                .FirstOrDefault();
            this.Series = curseries.Title;
        }

        ReviewMessages = new List<ReviewMessage>();

        var ListMessages = _context.ItemSheetReviewMessages.Where(isrm => isrm.Isactive == true 
          && isrm.ItemsheetId == sheet.Id).ToList();

        hasreview = false;
        foreach (var message in ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

    }

    public IteSheet(ItemSheetApproved sheet, NexusLarpLocalContext _context)
    {
        JsonElement tagslist = new JsonElement();
        Tags = new List<TagOut>();

        if (sheet.Taglists != null && sheet.Taglists != string.Empty)
        {
            TagsObject tagslists = JsonConvert.DeserializeObject<TagsObject>(sheet.Taglists);

            foreach (var tag in tagslists.MainTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag)
                    .Include("Tagtype").FirstOrDefault();
                this.Tags.Add(new TagOut(fullTag));
            }
        }

        if (sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.ItemtypeGuid).FirstOrDefault();
            this.Type = thistype.Type;
            this.ItemTypeGuid = (Guid)sheet.ItemtypeGuid;
        }
        else
        {
            Type = "Generic";
        }

        if (sheet.Fields != null)
        {
            Fields = JObject.Parse(sheet.Fields.RootElement.ToString());
            if (this.Tags.Count == 0)
            {
                var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(sheet.Fields.RootElement.ToString()));
                Fields = FeildsWInit;
                sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        var fullTag = _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .Include("Tagtype").FirstOrDefault();
                        this.Tags.Add(new TagOut(fullTag));
                    }
                }
            }
        }

        Id = sheet.Id;
        Guid = sheet.Guid;
        Isactive = sheet.Isactive;
        Name = sheet.Name;
        Img1 = sheet.Img1;
        Seriesguid = sheet.Seriesguid;
        Createdate = sheet.Createdate;
        CreatedbyuserGuid = sheet.CreatedbyuserGuid;
        FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid;
        SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid;
        EditbyUserGuid = sheet.EditbyUserGuid;
        Version = sheet.Version;
        readyforapproval = false;
        Gmnotes = sheet.Gmnotes;
        Isdoubleside = (bool)sheet.Isdoubleside;
        Back = new Backside(sheet);

        if (this.Type != null)
        {
            this.Back.Type = this.Type;
        }

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

        if (this.Seriesguid != null)
        {
            var curseries = _context.Series
                .Where(u => u.Guid == this.Seriesguid)
                .FirstOrDefault();
            this.Series = curseries.Title;
        }

        ReviewMessages = new List<ReviewMessage>();
        hasreview = false;
        var ListMessages = _context.ItemSheetReviewMessages.Where(isrm => isrm.Isactive == true
          && isrm.ItemsheetId == sheet.ItemsheetId).ToList();

        foreach (var message in ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

    }

    public IteSheet(ItemSheetDO sheet, NexusLarpLocalContext _context)
    {
        Tags = new List<TagOut>();

        if (sheet.TagList != null)
        {
            foreach (var tag in sheet.TagList)
            {
                this.Tags.Add(new TagOut(tag));
            }
        }

        if (sheet.Sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.Sheet.ItemtypeGuid).FirstOrDefault();
            this.Type = thistype.Type;
            this.ItemTypeGuid = (Guid)sheet.Sheet.ItemtypeGuid;
        }
        else
        {
            Type = "Generic";
        }

        Id = sheet.Sheet.Id;
        Isactive = sheet.Sheet.Isactive;
        Guid = sheet.Sheet.Guid;
        Name = sheet.Sheet.Name;
        Img1 = sheet.Sheet.Img1;
        Seriesguid = sheet.Sheet.Seriesguid;
        Createdate = sheet.Sheet.Createdate;
        CreatedbyuserGuid = sheet.Sheet.CreatedbyuserGuid;
        FirstapprovalbyuserGuid = sheet.Sheet.FirstapprovalbyuserGuid;
        SecondapprovalbyuserGuid = sheet.Sheet.SecondapprovalbyuserGuid;
        EditbyUserGuid = sheet.Sheet.EditbyUserGuid;
        Version = sheet.Sheet.Version;
        Back = new Backside(sheet.Sheet);

        if (this.Type != null)
        {
            this.Back.Type = this.Type;
        }

        if (sheet.Sheet.Readyforapproval != null)
        {
            readyforapproval = sheet.Sheet.Readyforapproval;
        }

        Gmnotes = sheet.Sheet.Gmnotes;

        if (this.Img1 != null)
        {
            if (System.IO.File.Exists(@"./images/items/UnApproved/" + sheet.Sheet.Img1))
            {
                this.imagedata =
                    System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Sheet.Img1);
            }
        }

        this.createdby = sheet.Createdbyuser?.Preferredname;
        if (sheet.Createdbyuser == null || sheet.Createdbyuser.Preferredname == null || sheet.Createdbyuser.Preferredname == string.Empty)
        {
            this.createdby = sheet.Createdbyuser?.Firstname;
        }

        if (sheet.Firstapprovalbyuser != null)
        {
            this.Firstapprovalby = sheet.Firstapprovalbyuser.Preferredname;
            if (sheet.Firstapprovalbyuser.Preferredname == null || sheet.Firstapprovalbyuser.Preferredname == string.Empty)
            {
                this.Firstapprovalby = sheet.Firstapprovalbyuser.Firstname;
            }
        }

        if (sheet.Secondapprovalbyuser != null)
        {
            this.Secondapprovalby = sheet.Secondapprovalbyuser.Preferredname;
            if (sheet.Secondapprovalbyuser.Preferredname == null || sheet.Secondapprovalbyuser.Preferredname == string.Empty)
            {
                this.Secondapprovalby = sheet.Secondapprovalbyuser.Firstname;
            }
        }

        this.EditbyUser = sheet.EditbyUser?.Preferredname;
        if (sheet.EditbyUser == null || sheet.EditbyUser.Preferredname == null || sheet.EditbyUser.Preferredname == string.Empty)
        {
            this.EditbyUser = sheet.EditbyUser?.Firstname;
        }

        if (sheet.Series != null)
        {
            this.Series = sheet.Series.Title;
        }
        
        ReviewMessages = new List<ReviewMessage>();

        hasreview = false;
        foreach (var message in sheet.ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

    }

    public IteSheet(ItemSheetApprovedDO sheet, NexusLarpLocalContext _context)
    {
        Tags = new List<TagOut>();

        if (sheet.TagList != null)
        {
            foreach (var tag in sheet.TagList)
            {
                this.Tags.Add(new TagOut(tag));
            }
        }

        if (sheet.Sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.Sheet.ItemtypeGuid).FirstOrDefault();
            this.Type = thistype.Type;
            this.ItemTypeGuid = (Guid)sheet.Sheet.ItemtypeGuid;
        }
        else
        {
            Type = "Generic";
        }

        Id = sheet.Sheet.Id;
        Guid = sheet.Sheet.Guid;
        Isactive = sheet.Sheet.Isactive;
        Name = sheet.Sheet.Name;
        Img1 = sheet.Sheet.Img1;
        Seriesguid = sheet.Sheet.Seriesguid;
        Createdate = sheet.Sheet.Createdate;
        CreatedbyuserGuid = sheet.Sheet.CreatedbyuserGuid;
        FirstapprovalbyuserGuid = sheet.Sheet.FirstapprovalbyuserGuid;
        SecondapprovalbyuserGuid = sheet.Sheet.SecondapprovalbyuserGuid;
        EditbyUserGuid = sheet.Sheet.EditbyUserGuid;
        Version = sheet.Sheet.Version;

        Gmnotes = sheet.Sheet.Gmnotes;
        Back = new Backside(sheet.Sheet);

        if (this.Type != null)
        {
            this.Back.Type = this.Type;
        }

        if (this.Img1 != null)
        {
            if (System.IO.File.Exists(@"./images/items/UnApproved/" + sheet.Sheet.Img1))
            {
                this.imagedata =
                    System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Sheet.Img1);
            }
        }

        this.createdby = sheet.Createdbyuser.Preferredname;
        if (sheet.Createdbyuser.Preferredname == null || sheet.Createdbyuser.Preferredname == string.Empty)
        {
            this.createdby = sheet.Createdbyuser.Firstname;
        }

        if (sheet.Firstapprovalbyuser != null)
        {
            this.Firstapprovalby = sheet.Firstapprovalbyuser.Preferredname;
            if (sheet.Firstapprovalbyuser.Preferredname == null || sheet.Firstapprovalbyuser.Preferredname == string.Empty)
            {
                this.Firstapprovalby = sheet.Firstapprovalbyuser.Firstname;
            }
        }

        if (sheet.Secondapprovalbyuser != null)
        {
            this.Secondapprovalby = sheet.Secondapprovalbyuser.Preferredname;
            if (sheet.Secondapprovalbyuser.Preferredname == null || sheet.Secondapprovalbyuser.Preferredname == string.Empty)
            {
                this.Secondapprovalby = sheet.Secondapprovalbyuser.Firstname;
            }
        }

        if (sheet.EditbyUser != null)
        {
            this.EditbyUser = sheet.EditbyUser.Preferredname;
            if (sheet.EditbyUser.Preferredname == null || sheet.EditbyUser.Preferredname == string.Empty)
            {
                this.EditbyUser = sheet.EditbyUser.Firstname;
            }
        }

        if (sheet.Series != null)
        {
            this.Series = sheet.Series.Title;
        }

        ReviewMessages = new List<ReviewMessage>();

        hasreview = false;
        foreach (var message in sheet.ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

    }

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
            Readyforapproval = this.readyforapproval,
            Isdoubleside = this.Isdoubleside

        };

        if (this.Fields != null)
        {
            output.Fields = JsonDocument.Parse(this.Fields.ToString());
        }

        return output;
    }

    public class Backside
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public JObject Fields { get; set; }

        public Backside()
        {

        }

        public Backside(ItemSheet iSheet)
        {
            this.Name = iSheet.Name;

            if (iSheet.Fields2ndside != null)
            {
                this.Fields = JObject.Parse(iSheet.Fields2ndside.RootElement.ToString());
            }
        }

        public Backside(ItemSheetApproved iSheet)
        {
            this.Name = iSheet.Name;

            if (iSheet.Fields2ndside != null)
            {
                this.Fields = JObject.Parse(iSheet.Fields2ndside.RootElement.ToString());
            }
        }
    }
}