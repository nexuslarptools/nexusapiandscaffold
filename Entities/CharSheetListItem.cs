using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using static Npgsql.PostgresTypes.PostgresCompositeType;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Entities
{
    public class CharSheetListItem
    {
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

        public CharSheetListItem(CharacterSheetApproved charSheet, NexusLarpLocalContext _context)
        {
            JsonElement tagslist = new JsonElement();
            tags = new List<TagOut>();

            if (charSheet.Taglists != null && charSheet.Taglists != string.Empty)
            {
                TagsObject tagslists = JsonConvert.DeserializeObject<TagsObject>(charSheet.Taglists);

                foreach (var tag in tagslists.MainTags)
                {
                    var fullTag = _context.Tags
                        .Where(t => t.Isactive == true && t.Guid == tag)
                        .FirstOrDefault();
                    this.tags.Add(new TagOut(fullTag));
                }
            }

            if (charSheet.Fields != null)
            {
                if (this.tags.Count == 0)
                {
                    charSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = charSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            var fullTag = _context.Tags
                                .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                                .FirstOrDefault();
                            this.tags.Add(new TagOut(fullTag));
                        }
                    }
                }
            }

            guid = charSheet.Guid;
            name = charSheet.Name;
            seriesguid = (Guid)charSheet.Seriesguid;
            title = _context.Series.Where(s => s.Guid == (Guid)charSheet.Seriesguid).FirstOrDefault().Title;  
            createdbyuserGuid = (Guid)charSheet.CreatedbyuserGuid;
            createdByUser = _context.Users.Where(u => u.Guid == (Guid)charSheet.CreatedbyuserGuid).FirstOrDefault().Preferredname;
            firstapprovalbyuserGuid = charSheet.FirstapprovalbyuserGuid;
            firstApprovalUser = IsNullOrEmpty(charSheet.FirstapprovalbyuserGuid) ? null : _context.Users.Where(u => u.Guid == (Guid)charSheet.FirstapprovalbyuserGuid).FirstOrDefault().Preferredname;
            secondapprovalbyuserGuid = charSheet.SecondapprovalbyuserGuid;
            secondApprovalUser = IsNullOrEmpty(charSheet.SecondapprovalbyuserGuid) ? null :  _context.Users.Where(u => u.Guid == (Guid)charSheet.SecondapprovalbyuserGuid).FirstOrDefault().Preferredname;
            editbyUser = IsNullOrEmpty(charSheet.EditbyUserGuid) ? null : _context.Users.Where(u => u.Guid == (Guid)charSheet.EditbyUserGuid).FirstOrDefault().Preferredname;
            hasreview = _context.CharacterSheetReviewMessages.Any(csrm => csrm.CharactersheetId == charSheet.CharactersheetId);
            readyforapproval = false;
        }

        public CharSheetListItem(CharacterSheet charSheet, NexusLarpLocalContext _context)
        {
            JsonElement tagslist = new JsonElement();
            tags = new List<TagOut>();

            if (charSheet.Taglists != null && charSheet.Taglists != string.Empty)
            {
                TagsObject tagslists = JsonConvert.DeserializeObject<TagsObject>(charSheet.Taglists);

                foreach (var tag in tagslists.MainTags)
                {
                    var fullTag = _context.Tags
                        .Where(t => t.Isactive == true && t.Guid == tag)
                        .FirstOrDefault();
                    this.tags.Add(new TagOut(fullTag));
                }
            }

            if (charSheet.Fields != null)
            {
                if (this.tags.Count == 0)
                {
                    charSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = charSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            var fullTag = _context.Tags
                                .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                                .FirstOrDefault();
                            this.tags.Add(new TagOut(fullTag));
                        }
                    }
                }
            }

            guid = charSheet.Guid;
            name = charSheet.Name;
            seriesguid = (Guid)charSheet.Seriesguid;
            title = _context.Series.Where(s => s.Guid == (Guid)charSheet.Seriesguid).FirstOrDefault().Title;
            createdbyuserGuid = (Guid)charSheet.CreatedbyuserGuid;
            createdByUser = _context.Users.Where(u => u.Guid == (Guid)charSheet.CreatedbyuserGuid).FirstOrDefault().Preferredname;
            firstapprovalbyuserGuid = charSheet.FirstapprovalbyuserGuid;
            firstApprovalUser = IsNullOrEmpty(charSheet.FirstapprovalbyuserGuid) ? null : _context.Users.Where(u => u.Guid == (Guid)charSheet.FirstapprovalbyuserGuid).FirstOrDefault().Preferredname;
            secondapprovalbyuserGuid = charSheet.SecondapprovalbyuserGuid;
            secondApprovalUser = IsNullOrEmpty(charSheet.SecondapprovalbyuserGuid) ? null : _context.Users.Where(u => u.Guid == (Guid)charSheet.SecondapprovalbyuserGuid).FirstOrDefault().Preferredname;
            editbyUser = IsNullOrEmpty(charSheet.EditbyUserGuid) ? null : _context.Users.Where(u => u.Guid == (Guid)charSheet.EditbyUserGuid).FirstOrDefault().Preferredname;
            hasreview = _context.CharacterSheetReviewMessages.Any(csrm => csrm.CharactersheetId == charSheet.Id);
            readyforapproval = charSheet.Readyforapproval;
        }

        public bool IsNullOrEmpty(Guid? input)
        {
            if (input == null || input == Guid.Empty)
            {
                return true;
            }

            return false;

        }
    }
}
