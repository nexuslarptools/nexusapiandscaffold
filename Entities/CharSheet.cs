using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Extensions;
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
    public List<Tag> Tags { get; set; }
    public byte[] imagedata1 { get; set; }
    public byte[] imagedata2 { get; set; }
    public Guid? EditbyUserGuid { get; set; }
    public string Editby { get; set; }

    public CharSheet()
    {
    }

    public CharSheet(CharacterSheetApproved input, NexusLarpLocalContext _context)
    {
        Guid = input.Guid;
        Seriesguid = input.Seriesguid;
        Name = input.Name;
        Img1 = input.Img1;
        Img2 = input.Img2;
        this.Fields = JObject.Parse(input.Fields.RootElement.ToString());
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

        var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == input.Seriesguid)
    .FirstOrDefault();

        this.SeriesTitle = assocSeries.Title;


        var sheet_item_guid = this.Fields["Sheet_Item"].ToString();

        if (sheet_item_guid != null)
        {
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                    .FirstOrDefault() != null)
                this.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());

            else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                         .FirstOrDefault() !=
                     null)
                this.Sheet_Item = Item.CreateItem(_context.ItemSheets
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
        }

        var Start_Items = new List<IteSheet>();

        var StartIguids = this.Fields["Starting_Items"].ToList();

        foreach (var iGuid in StartIguids)
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault() != null)
            {
                var starting_I = _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                    issh.Guid.ToString() == iGuid.ToString()).FirstOrDefault();


                Start_Items.Add(Item.CreateItem(starting_I));
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

                Start_Items.Add(Item.CreateItem(starting_I));
            }

        if (Start_Items != null) this.Starting_Items = Start_Items;

        if (this.CreatedbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.CreatedbyUserGuid)
                .FirstOrDefault();

            this.createdby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (this.FirstapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.FirstapprovalbyUserGuid)
                .FirstOrDefault();
            this.Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (this.SecondapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.SecondapprovalbyUserGuid)
                .FirstOrDefault();
            this.Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (this.EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.EditbyUserGuid)
                .FirstOrDefault();
            this.Editby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.Editby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }
    }

    public CharSheet(CharacterSheet input, NexusLarpLocalContext _context)
    {
        Guid = input.Guid;
        Seriesguid = input.Seriesguid;
        Name = input.Name;
        Img1 = input.Img1;
        Img2 = input.Img2;
        this.Fields = JObject.Parse(input.Fields.RootElement.ToString());
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

        var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == input.Seriesguid)
          .FirstOrDefault();

        this.SeriesTitle = assocSeries.Title;

        var sheet_item_guid = this.Fields["Sheet_Item"].ToString();

        if (sheet_item_guid != null)
        {
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                    .FirstOrDefault() != null)
                this.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());

            else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                         .FirstOrDefault() !=
                     null)
                this.Sheet_Item = Item.CreateItem(_context.ItemSheets
                    .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
        }

        var Start_Items = new List<IteSheet>();

        var StartIguids = this.Fields["Starting_Items"].ToList();

        foreach (var iGuid in StartIguids)
            if (_context.ItemSheetApproveds
                    .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                    .FirstOrDefault() != null)
            {
                var starting_I = _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                    issh.Guid.ToString() == iGuid.ToString()).FirstOrDefault();


                Start_Items.Add(Item.CreateItem(starting_I));
            }
            else if (_context.ItemSheets
                         .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                         .FirstOrDefault() != null)
            {
                var starting_I = _context.ItemSheets.Where(issh => issh.Isactive == true &&
                                     issh.Guid.ToString() == iGuid.ToString()).FirstOrDefault();

                Start_Items.Add(Item.CreateItem(starting_I));
            }

        if (Start_Items != null) this.Starting_Items = Start_Items;

        if (this.CreatedbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.CreatedbyUserGuid)
                .FirstOrDefault();

            this.createdby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (this.FirstapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.FirstapprovalbyUserGuid)
                .FirstOrDefault();
            this.Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (this.SecondapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.SecondapprovalbyUserGuid)
                .FirstOrDefault();
            this.Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (this.EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == this.EditbyUserGuid)
                .FirstOrDefault();
            this.Editby = lookupuser.Preferredname;
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                this.Editby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

    }

    public CharacterSheet OutputToCharacterSheet() 
    {
        CharacterSheet Charsheet = new CharacterSheet() {
            Guid = Guid,
            Seriesguid = (Guid)this.Seriesguid,
            Name = this.Name,
            Img1 = this.Img1,
            Img2 = this.Img2,
            Fields = JsonDocument.Parse(this.Fields.ToString()),
            Isactive = this.Isactive,
            Createdate = this.Createdate,
            EditbyUserGuid = this.EditbyUserGuid,
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