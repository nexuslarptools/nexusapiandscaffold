using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class CharSheetListItem
{
    public CharSheetListItem(CharacterSheetApproved charSheet, NexusLarpLocalContext _context)
    {
        var tagslist = new JsonElement();
        tags = new List<TagOut>();

        if (charSheet.Taglists != null && charSheet.Taglists != string.Empty)
        {
            var tagslists = JsonConvert.DeserializeObject<TagsObject>(charSheet.Taglists);

            foreach (var tag in tagslists.MainTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag)
                    .Include("Tagtype").FirstOrDefault();
                tags.Add(new TagOut(fullTag));
            }
        }

        if (charSheet.Fields != null)
            if (tags.Count == 0)
            {
                charSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = charSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        var fullTag = _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .Include("Tagtype").FirstOrDefault();
                        tags.Add(new TagOut(fullTag));
                    }
                }
            }

        guid = charSheet.Guid;
        name = charSheet.Name;
        seriesguid = (Guid)charSheet.Seriesguid;
        title = _context.Series.Where(s => s.Guid == (Guid)charSheet.Seriesguid).FirstOrDefault().Title;
        createdbyuserGuid = (Guid)charSheet.CreatedbyuserGuid;
        createdByUser = _context.Users.Where(u => u.Guid == (Guid)charSheet.CreatedbyuserGuid).FirstOrDefault()
            .Preferredname;
        firstapprovalbyuserGuid = charSheet.FirstapprovalbyuserGuid;
        firstApprovalUser = IsNullOrEmpty(charSheet.FirstapprovalbyuserGuid)
            ? null
            : _context.Users.Where(u => u.Guid == (Guid)charSheet.FirstapprovalbyuserGuid).FirstOrDefault()
                .Preferredname;
        secondapprovalbyuserGuid = charSheet.SecondapprovalbyuserGuid;
        secondApprovalUser = IsNullOrEmpty(charSheet.SecondapprovalbyuserGuid)
            ? null
            : _context.Users.Where(u => u.Guid == (Guid)charSheet.SecondapprovalbyuserGuid).FirstOrDefault()
                .Preferredname;
        editbyUser = IsNullOrEmpty(charSheet.EditbyUserGuid)
            ? null
            : _context.Users.Where(u => u.Guid == (Guid)charSheet.EditbyUserGuid).FirstOrDefault().Preferredname;
        hasreview = _context.CharacterSheetReviewMessages.Any(csrm =>
            csrm.CharactersheetId == charSheet.CharactersheetId);
        readyforapproval = false;
    }

    public CharSheetListItem(CharacterSheetApprovedDO charSheet, NexusLarpLocalContext _context)
    {
        var tagslist = new JsonElement();
        tags = new List<TagOut>();


        foreach (var tag in charSheet.TagList)
            if (tag.Tagtypeguid == Guid.Parse("26cd7510-9401-11ea-899a-4fd87913c65d"))
                tags.Add(new TagOut(tag));

        guid = charSheet.Sheet.Guid;
        name = charSheet.Sheet.Name;
        seriesguid = charSheet.Series.Guid;
        title = charSheet.Series.Title;
        createdbyuserGuid = charSheet.Createdbyuser.Guid;
        createdByUser = charSheet.Createdbyuser.Preferredname;
        firstapprovalbyuserGuid = charSheet.Firstapprovalbyuser == null ? null : charSheet.Firstapprovalbyuser.Guid;
        firstApprovalUser = charSheet.Firstapprovalbyuser == null ? null : charSheet.Firstapprovalbyuser.Preferredname;
        secondapprovalbyuserGuid = charSheet.Secondapprovalbyuser == null ? null : charSheet.Secondapprovalbyuser.Guid;
        ;
        secondApprovalUser =
            charSheet.Secondapprovalbyuser == null ? null : charSheet.Secondapprovalbyuser.Preferredname;
        editbyUser = charSheet.EditbyUser == null ? null : charSheet.EditbyUser.Preferredname;
        hasreview = charSheet.CharacterSheetReviewMessages.Count > 0 ? true : false;
    }

    public CharSheetListItem(CharacterSheet charSheet, NexusLarpLocalContext _context)
    {
        var tagslist = new JsonElement();
        tags = new List<TagOut>();

        if (charSheet.Taglists != null && charSheet.Taglists != string.Empty)
        {
            var tagslists = JsonConvert.DeserializeObject<TagsObject>(charSheet.Taglists);

            foreach (var tag in tagslists.MainTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag)
                    .Include("Tagtype").FirstOrDefault();
                tags.Add(new TagOut(fullTag));
            }
        }

        if (charSheet.Fields != null)
            if (tags.Count == 0)
            {
                charSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = charSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        var fullTag = _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .Include("Tagtype").FirstOrDefault();
                        tags.Add(new TagOut(fullTag));
                    }
                }
            }

        guid = charSheet.Guid;
        name = charSheet.Name;
        seriesguid = charSheet.Seriesguid;
        title = _context.Series.Where(s => s.Guid == charSheet.Seriesguid).FirstOrDefault().Title;
        createdbyuserGuid = (Guid)charSheet.CreatedbyuserGuid;
        createdByUser = _context.Users.Where(u => u.Guid == (Guid)charSheet.CreatedbyuserGuid).FirstOrDefault()
            .Preferredname;
        firstapprovalbyuserGuid = charSheet.FirstapprovalbyuserGuid;
        firstApprovalUser = IsNullOrEmpty(charSheet.FirstapprovalbyuserGuid)
            ? null
            : _context.Users.Where(u => u.Guid == (Guid)charSheet.FirstapprovalbyuserGuid).FirstOrDefault()
                .Preferredname;
        secondapprovalbyuserGuid = charSheet.SecondapprovalbyuserGuid;
        secondApprovalUser = IsNullOrEmpty(charSheet.SecondapprovalbyuserGuid)
            ? null
            : _context.Users.Where(u => u.Guid == (Guid)charSheet.SecondapprovalbyuserGuid).FirstOrDefault()
                .Preferredname;
        editbyUser = IsNullOrEmpty(charSheet.EditbyUserGuid)
            ? null
            : _context.Users.Where(u => u.Guid == (Guid)charSheet.EditbyUserGuid).FirstOrDefault().Preferredname;
        hasreview = _context.CharacterSheetReviewMessages.Any(csrm => csrm.CharactersheetId == charSheet.Id);
        readyforapproval = charSheet.Readyforapproval;
    }

    public CharSheetListItem(CharacterSheetDO charSheet, NexusLarpLocalContext _context)
    {
        var tagslist = new JsonElement();
        tags = new List<TagOut>();


        foreach (var tag in charSheet.TagList)
            if (tag.Tagtypeguid == Guid.Parse("26cd7510-9401-11ea-899a-4fd87913c65d"))
                tags.Add(new TagOut(tag));


        guid = charSheet.Sheet.Guid;
        name = charSheet.Sheet.Name;
        seriesguid = charSheet.Series.Guid;
        title = charSheet.Series.Title;
        createdbyuserGuid = charSheet.Createdbyuser.Guid;
        createdByUser = charSheet.Createdbyuser.Preferredname;
        firstapprovalbyuserGuid = charSheet.Firstapprovalbyuser == null ? null : charSheet.Firstapprovalbyuser.Guid;
        firstApprovalUser = charSheet.Firstapprovalbyuser == null ? null : charSheet.Firstapprovalbyuser.Preferredname;
        secondapprovalbyuserGuid = charSheet.Secondapprovalbyuser == null ? null : charSheet.Secondapprovalbyuser.Guid;
        ;
        secondApprovalUser =
            charSheet.Secondapprovalbyuser == null ? null : charSheet.Secondapprovalbyuser.Preferredname;
        editbyUser = charSheet.EditbyUser == null ? null : charSheet.EditbyUser.Preferredname;
        hasreview = charSheet.CharacterSheetReviewMessages.Count > 0 ? true : false;
        readyforapproval = charSheet.Sheet.Readyforapproval;
    }

    public Guid guid { get; set; }
    public string name { get; set; }
    public Guid seriesguid { get; set; }
    public string title { get; set; }
    public Guid createdbyuserGuid { get; set; }
    public string createdByUser { get; set; }
    public Guid? firstapprovalbyuserGuid { get; set; }
    public string firstApprovalUser { get; set; }
    public Guid? secondapprovalbyuserGuid { get; set; }
    public string secondApprovalUser { get; set; }
    public string editbyUser { get; set; }
    public bool readyforapproval { get; set; }
    public List<TagOut> tags { get; set; }
    public bool hasreview { get; set; }

    public bool IsNullOrEmpty(Guid? input)
    {
        if (input == null || input == Guid.Empty) return true;

        return false;
    }
}