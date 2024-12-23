﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class IteSheet
{
    private NexusLarpLocalContext context;
    private object sheet;
    private List<string> _largeItems = new List<string>(new string[] {
         "mecha",
        "companion",
        "pokemon",
        "vehicle" 
    });


    public IteSheet()
    {
    }

    public IteSheet(ItemSheet sheet, NexusLarpLocalContext _context)
    {
        var tagslist = new JsonElement();
        Tags = new List<TagOut>();

        if (sheet.ItemSheetTags != null)
            foreach (var tag in sheet.ItemSheetTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag.TagGuid)
                    .Include("Tagtype").FirstOrDefault();
                if (fullTag != null) Tags.Add(new TagOut(fullTag));
            }

        if (sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.ItemtypeGuid).FirstOrDefault();
            Type = thistype.Type;
            ItemTypeGuid = (Guid)sheet.ItemtypeGuid;
        }
        else
        {
            Type = "Generic";
        }

        if (sheet.Fields != null)
        {
            Fields = JObject.Parse(sheet.Fields.RootElement.ToString());
            if (Tags.Count == 0)
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
        Isdoubleside = sheet.Isdoubleside == null ? false: (bool)sheet.Isdoubleside;
        Back = new Backside(sheet);
        Issheetitem = false;

        if (Type != null) Back.Type = Type;


        //if (Img1 != null)
         //   if (File.Exists(@"./images/items/UnApproved/" + sheet.Img1))
          //      imagedata =
          //          File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);

        if (CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == CreatedbyuserGuid)
                .FirstOrDefault();
            createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) createdby = creUser.Firstname;
        }

        if (FirstapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == FirstapprovalbyuserGuid)
                .FirstOrDefault();
            Firstapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                Firstapprovalby = creUser.Firstname;
        }

        if (SecondapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == SecondapprovalbyuserGuid)
                .FirstOrDefault();
            Secondapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                Secondapprovalby = creUser.Firstname;
        }

        if (EditbyUserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == EditbyUserGuid)
                .FirstOrDefault();
            EditbyUser = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) EditbyUser = creUser.Firstname;
        }

        if (Seriesguid != null)
        {
            var curseries = _context.Series
                .Where(u => u.Guid == Seriesguid)
                .FirstOrDefault();
            Series = curseries.Title;
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

        this.SetIsLarge();
    }

    public IteSheet(ItemSheetApproved sheet, NexusLarpLocalContext _context)
    {
        var tagslist = new JsonElement();
        Tags = new List<TagOut>();

        if (sheet.Taglists != null && sheet.Taglists != string.Empty)
        {
            var tagslists = JsonConvert.DeserializeObject<TagsObject>(sheet.Taglists);

            foreach (var tag in tagslists.MainTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag)
                    .Include("Tagtype").FirstOrDefault();
                Tags.Add(new TagOut(fullTag));
            }
        }

        if (sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.ItemtypeGuid).FirstOrDefault();
            Type = thistype.Type;
            ItemTypeGuid = (Guid)sheet.ItemtypeGuid;
        }
        else
        {
            Type = "Generic";
        }

        if (sheet.Fields != null)
        {
            Fields = JObject.Parse(sheet.Fields.RootElement.ToString());
            if (Tags.Count == 0)
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
                        Tags.Add(new TagOut(fullTag));
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
        Isdoubleside = sheet.Isdoubleside == null ? false : (bool)sheet.Isdoubleside;
        Back = new Backside(sheet);
        Issheetitem = false;

        if (Type != null) Back.Type = Type;

        //if (Img1 != null)
          //  if (File.Exists(@"./images/items/UnApproved/" + sheet.Img1))
          //      imagedata =
           //         File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);

        if (CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == CreatedbyuserGuid)
                .FirstOrDefault();
            createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) createdby = creUser.Firstname;
        }

        if (FirstapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == FirstapprovalbyuserGuid)
                .FirstOrDefault();
            Firstapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                Firstapprovalby = creUser.Firstname;
        }

        if (SecondapprovalbyuserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == SecondapprovalbyuserGuid)
                .FirstOrDefault();
            Secondapprovalby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                Secondapprovalby = creUser.Firstname;
        }

        if (EditbyUserGuid != null)
        {
            var creUser = _context.Users
                .Where(u => u.Guid == EditbyUserGuid)
                .FirstOrDefault();
            EditbyUser = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) EditbyUser = creUser.Firstname;
        }

        if (Seriesguid != null)
        {
            var curseries = _context.Series
                .Where(u => u.Guid == Seriesguid)
                .FirstOrDefault();
            Series = curseries.Title;
        }

        ReviewMessages = new List<ReviewMessage>();
        hasreview = false;
        var ListMessages = _context.ItemSheetReviewMessages.Where(isrm => isrm.Isactive == true
                                                                          && isrm.ItemsheetId == sheet.ItemsheetId)
            .ToList();

        foreach (var message in ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

        this.SetIsLarge();
    }

    public IteSheet(ItemSheetDO sheet, NexusLarpLocalContext _context)
    {
        Tags = new List<TagOut>();

        if (sheet.TagList != null)
            foreach (var tag in sheet.TagList)
                Tags.Add(new TagOut(tag));

        if (sheet.Sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.Sheet.ItemtypeGuid).FirstOrDefault();
            Type = thistype.Type;
            ItemTypeGuid = (Guid)sheet.Sheet.ItemtypeGuid;
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
        Firstapprovaldate  = sheet.Sheet.Firstapprovaldate;
        Secondapprovaldate = sheet.Sheet.Secondapprovaldate;
        EditbyUserGuid = sheet.Sheet.EditbyUserGuid;
        Version = sheet.Sheet.Version;
        Back = new Backside(sheet.Sheet);
        Issheetitem = false;

        if (Type != null) Back.Type = Type;

        if (sheet.Sheet.Readyforapproval != null) readyforapproval = sheet.Sheet.Readyforapproval;

        Gmnotes = sheet.Sheet.Gmnotes;

        //if (Img1 != null)
          //  if (File.Exists(@"./images/items/UnApproved/" + sheet.Sheet.Img1))
           //     imagedata =
           //         File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Sheet.Img1);

        createdby = sheet.Createdbyuser?.Preferredname;
        if (sheet.Createdbyuser == null || sheet.Createdbyuser.Preferredname == null ||
            sheet.Createdbyuser.Preferredname == string.Empty) createdby = sheet.Createdbyuser?.Firstname;

        if (sheet.Firstapprovalbyuser != null)
        {
            Firstapprovalby = sheet.Firstapprovalbyuser.Preferredname;
            if (sheet.Firstapprovalbyuser.Preferredname == null ||
                sheet.Firstapprovalbyuser.Preferredname == string.Empty)
                Firstapprovalby = sheet.Firstapprovalbyuser.Firstname;
        }

        if (sheet.Secondapprovalbyuser != null)
        {
            Secondapprovalby = sheet.Secondapprovalbyuser.Preferredname;
            if (sheet.Secondapprovalbyuser.Preferredname == null ||
                sheet.Secondapprovalbyuser.Preferredname == string.Empty)
                Secondapprovalby = sheet.Secondapprovalbyuser.Firstname;
        }

        EditbyUser = sheet.EditbyUser?.Preferredname;
        if (sheet.EditbyUser == null || sheet.EditbyUser.Preferredname == null ||
            sheet.EditbyUser.Preferredname == string.Empty) EditbyUser = sheet.EditbyUser?.Firstname;

        if (sheet.Series != null) Series = sheet.Series.Title;

        ReviewMessages = new List<ReviewMessage>();

        hasreview = false;
        foreach (var message in sheet.ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

        this.WasApproved = _context.ItemSheetApproveds.Any(isa => isa.Guid == sheet.Sheet.Guid);

        this.SetIsLarge();
    }

    public IteSheet(ItemSheetApprovedDO sheet, NexusLarpLocalContext _context)
    {
        Tags = new List<TagOut>();

        if (sheet.TagList != null)
            foreach (var tag in sheet.TagList)
                Tags.Add(new TagOut(tag));

        if (sheet.Sheet.ItemtypeGuid != null)
        {
            var thistype = _context.ItemTypes.Where(i => i.Guid == (Guid)sheet.Sheet.ItemtypeGuid).FirstOrDefault();
            Type = thistype.Type;
            ItemTypeGuid = (Guid)sheet.Sheet.ItemtypeGuid;
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
        Issheetitem = false;

        Gmnotes = sheet.Sheet.Gmnotes;
        Back = new Backside(sheet.Sheet);

        if (Type != null) Back.Type = Type;

        //if (Img1 != null)
          //  if (File.Exists(@"./images/items/UnApproved/" + sheet.Sheet.Img1))
            //    imagedata =
               //     File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Sheet.Img1);

        createdby = sheet.Createdbyuser.Preferredname;
        if (sheet.Createdbyuser.Preferredname == null || sheet.Createdbyuser.Preferredname == string.Empty)
            createdby = sheet.Createdbyuser.Firstname;

        if (sheet.Firstapprovalbyuser != null)
        {
            Firstapprovalby = sheet.Firstapprovalbyuser.Preferredname;
            if (sheet.Firstapprovalbyuser.Preferredname == null ||
                sheet.Firstapprovalbyuser.Preferredname == string.Empty)
                Firstapprovalby = sheet.Firstapprovalbyuser.Firstname;
        }

        if (sheet.Secondapprovalbyuser != null)
        {
            Secondapprovalby = sheet.Secondapprovalbyuser.Preferredname;
            if (sheet.Secondapprovalbyuser.Preferredname == null ||
                sheet.Secondapprovalbyuser.Preferredname == string.Empty)
                Secondapprovalby = sheet.Secondapprovalbyuser.Firstname;
        }

        if (sheet.EditbyUser != null)
        {
            EditbyUser = sheet.EditbyUser.Preferredname;
            if (sheet.EditbyUser.Preferredname == null || sheet.EditbyUser.Preferredname == string.Empty)
                EditbyUser = sheet.EditbyUser.Firstname;
        }

        if (sheet.Series != null) Series = sheet.Series.Title;

        ReviewMessages = new List<ReviewMessage>();

        hasreview = false;
        foreach (var message in sheet.ListMessages)
        {
            hasreview = true;
            ReviewMessages.Add(new ReviewMessage(message, _context));
        }

        this.SetIsLarge();
    }

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
    //public byte[] imagedata { get; set; }
    public Guid? EditbyUserGuid { get; set; }
    public string EditbyUser { get; set; }
    public bool readyforapproval { get; set; }
    public bool hasreview { get; set; }
    public bool Islarge { get; set; }
    public bool Issheetitem { get; set; }
    public bool Isdoubleside { get; set; }
    public bool Isfrontonly { get; set; }
    public bool Isbackonly { get; set; }
    public Backside Back { get; set; }
    public bool WasApproved { get; set; }

    public List<ReviewMessage> ReviewMessages { get; set; }

    public ItemSheet OutputToItemSheet()
    {
        var output = new ItemSheet
        {
            Version = 1,
            Guid = Guid,
            Id = Id,
            Seriesguid = Seriesguid,
            Name = Name,
            Img1 = Img1,
            Fields = null,
            Isactive = true,
            CreatedbyuserGuid = CreatedbyuserGuid,
            FirstapprovalbyuserGuid = FirstapprovalbyuserGuid,
            Firstapprovaldate = Firstapprovaldate,
            Secondapprovaldate = Secondapprovaldate,
            SecondapprovalbyuserGuid = SecondapprovalbyuserGuid,
            Gmnotes = Gmnotes,
            Reason4edit = Reason4edit,
            Readyforapproval = readyforapproval,
            Isdoubleside = Isdoubleside
        };

        if (Fields != null) output.Fields = JsonDocument.Parse(Fields.ToString());

        return output;
    }

    public class Backside
    {
        public Backside()
        {
        }

        public Backside(ItemSheet iSheet)
        {
            Name = iSheet.Name;

            if (iSheet.Fields2ndside != null) Fields = JObject.Parse(iSheet.Fields2ndside.RootElement.ToString());
        }

        public Backside(ItemSheetApproved iSheet)
        {
            Name = iSheet.Name;

            if (iSheet.Fields2ndside != null) Fields = JObject.Parse(iSheet.Fields2ndside.RootElement.ToString());
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public JObject Fields { get; set; }
    }

    public void SetIsLarge()
    {
        this.Islarge = false;

        if (this.Type != null && _largeItems.Contains(this.Type.ToLower()))
        {
            this.Islarge = true;
        }
    }
}