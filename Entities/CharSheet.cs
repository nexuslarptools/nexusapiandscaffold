using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class CharSheet
{
    public CharSheet()
    {
    }

    public CharSheet(CharacterSheetApproved input, NexusLarpLocalContext _context)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(input.Fields.RootElement.ToString()));

        Id = input.Id;
        Guid = input.Guid;
        Seriesguid = input.Seriesguid;
        Name = input.Name;
        Img1 = input.Img1;
        Img2 = input.Img2;
        Fields = FeildsWInit;
        Isactive = input.Isactive;
        Createdate = input.Createdate;
        CreatedbyUserGuid = input.CreatedbyuserGuid;
        EditbyUserGuid = input.EditbyUserGuid;
        FirstapprovalbyUserGuid = input.FirstapprovalbyuserGuid;
        Firstapprovaldate = input.Firstapprovaldate;
        SecondapprovalbyUserGuid = input.SecondapprovalbyuserGuid;
        Secondapprovaldate = input.Secondapprovaldate;
        Gmnotes = input.Gmnotes;
        Reason4edit = input.Reason4edit;
        Version = input.Version;
        Readyforapproval = false;
        HasReview = false;

        var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == input.Seriesguid)
            .FirstOrDefault();

        SeriesTitle = assocSeries.Title;


        var sheet_item_guid = Fields["Sheet_Item"].ToString();

        if (sheet_item_guid != null)
        {
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                    .FirstOrDefault() != null)
                Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault(),
                    _context);

            else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                         .FirstOrDefault() !=
                     null)
                Sheet_Item = Item.CreateItem(_context.ItemSheets
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault(),
                    _context);
        }

        var Start_Items = new List<IteSheet>();

        var StartIguids = Fields["Starting_Items"].ToList();

        foreach (var iGuid in StartIguids)
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault() != null)
            {
                var starting_I = _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                                                                           issh.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault();


                Start_Items.Add(Item.CreateItem(starting_I, _context));
            }
            else if (_context.ItemSheets
                         .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                         .FirstOrDefault() != null)
            {
                //Start_Items.Add(JObject.Parse(_context.ItemSheet.Where(isa => isa.Isactive == true
                //&& isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault().Fields.RootElement.ToString()));
                var starting_I = _context.ItemSheets.Where(issh => issh.Isactive == true &&
                                                                   issh.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault();

                Start_Items.Add(Item.CreateItem(starting_I, _context));
            }

        if (Start_Items != null) Starting_Items = Start_Items;

        if (CreatedbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == CreatedbyUserGuid)
                .FirstOrDefault();

            createdby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (FirstapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == FirstapprovalbyUserGuid)
                .FirstOrDefault();
            Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (SecondapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == SecondapprovalbyUserGuid)
                .FirstOrDefault();
            Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == EditbyUserGuid)
                .FirstOrDefault();
            Editby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                Editby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        ReviewMessages = new List<ReviewMessage>();

        var ListId = _context.CharacterSheets.Where(ish => ish.Guid == input.Guid).Select(x => x.Id).ToList();
        var ListMessages = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Isactive == true
            && ListId.Contains(isrm.CharactersheetId)).ToList();

        if (ListMessages.Count() > 0)
        {
            HasReview = true;
        }
    }

    public CharSheet(CharacterSheet input, NexusLarpLocalContext _context)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(input.Fields.RootElement.ToString()));

        Id = input.Id;
        Guid = input.Guid;
        Seriesguid = input.Seriesguid;
        Name = input.Name;
        Img1 = input.Img1;
        Img2 = input.Img2;
        Fields = FeildsWInit;
        Isactive = input.Isactive;
        Createdate = input.Createdate;
        CreatedbyUserGuid = input.CreatedbyuserGuid;
        EditbyUserGuid = input.EditbyUserGuid;
        FirstapprovalbyUserGuid = input.FirstapprovalbyuserGuid;
        Firstapprovaldate = input.Firstapprovaldate;
        SecondapprovalbyUserGuid = input.SecondapprovalbyuserGuid;
        Secondapprovaldate = input.Secondapprovaldate;
        Gmnotes = input.Gmnotes;
        Reason4edit = input.Reason4edit;
        Version = input.Version;
        Readyforapproval = input.Readyforapproval;

        var assocSeries = _context.Series.Where(s => s.Guid == input.Seriesguid)
            .FirstOrDefault();

        SeriesTitle = assocSeries.Title;

        var sheet_item_guid = Fields["Sheet_Item"].ToString();

        if (sheet_item_guid != null)
        {
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                    .FirstOrDefault() != null)
                Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault(),
                    _context);

            else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                         .FirstOrDefault() !=
                     null)
                Sheet_Item = Item.CreateItem(_context.ItemSheets
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault(),
                    _context);
        }

        var Start_Items = new List<IteSheet>();

        var StartIguids = Fields["Starting_Items"].ToList();

        foreach (var iGuid in StartIguids)
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault() != null)
            {
                var starting_I = _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                                                                           issh.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault();


                Start_Items.Add(Item.CreateItem(starting_I, _context));
            }
            else if (_context.ItemSheets
                         .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                         .FirstOrDefault() != null)
            {
                var starting_I = _context.ItemSheets.Where(issh => issh.Isactive == true &&
                                                                   issh.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault();

                Start_Items.Add(Item.CreateItem(starting_I, _context));
            }

        if (Start_Items != null) Starting_Items = Start_Items;

        if (CreatedbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == CreatedbyUserGuid)
                .FirstOrDefault();

            createdby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (FirstapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == FirstapprovalbyUserGuid)
                .FirstOrDefault();
            Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (SecondapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == SecondapprovalbyUserGuid)
                .FirstOrDefault();
            Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == EditbyUserGuid)
                .FirstOrDefault();
            Editby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                Editby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        ReviewMessages = new List<ReviewMessage>();

        var ListId = _context.CharacterSheets.Where(ish => ish.Guid == input.Guid).Select(x => x.Id).ToList();
        var ListMessages = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Isactive == true
            && ListId.Contains(isrm.CharactersheetId)).ToList();

        if (ListMessages.Count() > 0)
        {
            HasReview = true;
        }
    }
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public Guid? Seriesguid { get; set; }
    public string SeriesTitle { get; set; }
    public string Name { get; set; }
    public string Img1 { get; set; }
    public string Img2 { get; set; }
    public JObject Fields { get; set; }
    public IteSheet Sheet_Item { get; set; }
    public List<IteSheet> Starting_Items { get; set; }
    public List<IteSheet> Upgrade_Items { get; set; }
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
    public List<TagOut> Tags { get; set; }
    public Guid? EditbyUserGuid { get; set; }
    public string Editby { get; set; }
    public bool HasReview { get; set; }
    public List<ReviewMessage> ReviewMessages { get; set; }
    public bool Readyforapproval { get; set; }


    public CharacterSheet OutputToCharacterSheet()
    {
        var Charsheet = new CharacterSheet
        {
            Guid = Guid,
            Seriesguid = (Guid)Seriesguid,
            Name = Name,
            Img1 = Img1,
            Img2 = Img2,
            Fields = JsonDocument.Parse(Fields.ToString()),
            Isactive = Isactive == null || (bool)Isactive,
            Createdate = Createdate,
            EditbyUserGuid = EditbyUserGuid,
            CreatedbyuserGuid = CreatedbyUserGuid,
            FirstapprovalbyuserGuid = FirstapprovalbyUserGuid,
            Firstapprovaldate = Firstapprovaldate,
            SecondapprovalbyuserGuid = SecondapprovalbyUserGuid,
            Secondapprovaldate = Secondapprovaldate,
            Gmnotes = Gmnotes,
            Reason4edit = Reason4edit,
            Readyforapproval = Readyforapproval
        };

        return Charsheet;
    }
}