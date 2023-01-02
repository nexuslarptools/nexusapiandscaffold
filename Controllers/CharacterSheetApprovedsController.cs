using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CharacterSheetApprovedsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public CharacterSheetApprovedsController(NexusLARPContextBase context)
        {
            _context = context;
        }


        /// <summary>
        /// Gets list of all active character sheets which exist in the approved characters table.
        /// </summary>
        /// <returns></returns>
        // GET: api/V1/CharacterSheetApproveds
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetApproved()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                var ret = await _context.CharacterSheetApproved.Where(cs => cs.Isactive == true && allowedSheets.Contains(cs.Guid)).Select(c => new { c.Guid, c.Name, c.Seriesguid, c.Seriesgu.Title }).ToListAsync();

                return Ok(ret);
            }
            return Unauthorized();

        }


        /// <summary>
        /// Gets list of all character sheets which exist in the Approved table, including diabled sheets
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/CharacterSheets
        [HttpGet("IncludeDeactive")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetApprovedCharacterSheetWithDisabled()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                var ret = await _context.CharacterSheetApproved.Select(c => new { c.Id, c.Guid, c.Name, c.Seriesguid, c.Seriesgu.Title, c.Isactive}).ToListAsync();

                return Ok(ret);
            }
            return Unauthorized();

        }


        // GET: api/CharacterSheetApproveds/5
        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<CharacterSheetApproved>> GetCharacterSheetApproved(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                if (!allowedSheets.Contains(guid))
                {
                    return Unauthorized();
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var characterSheet = await _context.CharacterSheetApproved.Where(cs => cs.Guid == guid && cs.Isactive == true).FirstOrDefaultAsync();

                if (characterSheet == null)
                {
                    return NotFound();
                }


                CharSheet outputSheet = Character.CreateCharSheet(characterSheet);


                return Ok(outputSheet);
            }
            return Unauthorized();
        }


        /// <summary>
        /// Returns all characters from a series.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        // GET: api/vi/CharacterSheets/{guid}
        [HttpGet("BySeries/{guid}")]
        [Authorize]
        public async Task<ActionResult<CharacterSheet>> GetCharacterSheetBySeries(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalseries = _context.Series.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Tags)).ToList();
                List<TagScanContainer> legalsheets = _context.Series.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Tags)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSeries = TagScanner.ScanTags(legalseries, allowedTags);
                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                if (!allowedSeries.Contains(guid))
                {
                    return Unauthorized();
                }



                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var characterSheet = await _context.CharacterSheetApproved.Where(cs => cs.Isactive == true && cs.Seriesguid == guid  && allowedSheets.Contains(cs.Guid))
                    .Select(c => new {
                        c.Guid,
                        c.Name,
                        c.Seriesguid,
                        c.Seriesgu.Title
                    }).ToListAsync();

                if (characterSheet == null)
                {
                    return NotFound();
                }

                return Ok(characterSheet);
            }
            return Unauthorized();
        }


        [HttpGet("ByAbility")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetApprovedCharacterSheetByAbilites([FromBody] JsonElement input)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                string json = System.Text.Json.JsonSerializer.Serialize(input);

                using var dynJson = JsonDocument.Parse(json);

                var chars = await _context.CharacterSheetApproved.Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

                foreach (var jo in dynJson.RootElement.EnumerateObject().GetEnumerator())
                {
                    chars = chars.Where(ch => ch.Fields.RootElement.GetProperty(jo.Name.ToString()).ToString() == jo.Value.ToString()).ToList();

                }

                var output = chars.Select(cha => new { cha.Guid, cha.Name, cha.Seriesguid, cha.Seriesgu.Title }).ToList();

                return Ok(output);
            }
            return Unauthorized();
        }



        [HttpGet("ByTag/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetApprovedCharacterSheetByTag(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                if (!allowedTags.Contains(guid))
                {
                    return Unauthorized();
                }

                var chars = await _context.CharacterSheetApproved.Where(c => c.Isactive == true && c.Fields.RootElement.GetProperty("Character_Tags").GetString().Contains(guid.ToString())).ToListAsync();


                var output = chars.Select(cha => new
                {
                    cha.Guid,
                    cha.Name,
                    SeriesGuid = cha.Seriesguid,
                    SeriesTitle = _context.Series.Where(s => s.Isactive == true && s.Guid == cha.Seriesguid).FirstOrDefault().Title
                }).ToList();

                return Ok(output);
            }
            return Unauthorized();
        }




        /// <summary>
        /// Search for character based on partial character name or alternate name.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        // GET: api/v1/Search
        [HttpGet("Search/{input}")]
        [Authorize]
        public async Task<ActionResult<Series>> GetApprovedCharacterSearchPartial(string input)
        {

            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                var testChar = await _context.CharacterSheetApproved.Where(ch => ch.Isactive == true && allowedSheets.Contains(ch.Guid)).Select(cha => new {
                    cha.Guid,
                    cha.Name,
                    cha.Seriesguid,
                    cha.Seriesgu.Title,
                    cha.Fields
                }).ToListAsync();

                var testQuery = testChar.Where(tc => tc.Fields.RootElement.GetProperty("Alternate Names").ToString().ToLower().Contains(input.ToLower()) ||
                tc.Name.ToLower().Contains(input.ToLower())).ToList();



                var characters = testQuery.Select(ch => new {
                    ch.Guid,
                    ch.Name,
                    ch.Seriesguid,
                    ch.Title
                }).ToList();

                if (characters == null)
                {
                    return NotFound();
                }


                return Ok(characters);
            }
            return Unauthorized();
        }




        [HttpGet("BySpecialSkillsTag/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetApprovedCharacterSheetBySpecialSkillsTag(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);


                List<Guid> allFound = new List<Guid>();

                var allSheets = await _context.CharacterSheetApproved.Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

                foreach (var sheet in allSheets)
                {
                    var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Special_Skills").EnumerateArray();


                    foreach (var Testing in TestJsonFeilds)
                    {
                        var tagresult = Testing.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in tagresult)
                        {
                            if (tag.ToString() == guid.ToString())
                            {

                                allFound.Add(sheet.Guid);
                            }
                        }

                    }
                }



                var output = await _context.CharacterSheetApproved.Where(c => c.Isactive == true &&
               allFound.Contains(c.Guid)).Select(ch => new
               {
                   ch.Name,
                   ch.Guid,
                   ch.Seriesguid,
                   SeriesTitle = _context.Series.Where(s => s.Isactive == true && s.Guid == ch.Seriesguid).FirstOrDefault().Title
               }).ToListAsync();


                return Ok(output);
            }
            return Unauthorized();
        }



        // GET: api/CharacterSheets/5/WithSheetItem
        [HttpGet("{guid}/withsheetitem")]
        [Authorize]
        public async Task<ActionResult<CharacterSheet>> GetApprovedCharacterSheetWithSheetItem(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                if (!allowedSheets.Contains(guid))
                {
                    return Unauthorized();
                }

                var characterSheet = await _context.CharacterSheetApproved.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


                CharSheet outputSheet = Character.CreateCharSheet(characterSheet);

                var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid).FirstOrDefault();

                outputSheet.SeriesTitle = assocSeries.Title;


                var sheet_item_guid = _context.CharacterSheetApproved.Where(c => c.Isactive == true && c.Guid == guid).FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


                if (sheet_item_guid != null)
                {

                    if (_context.ItemSheetApproved.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault() != null)
                    {
                        outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproved.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());

                    }

                    else if (_context.ItemSheet.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault() != null)
                    {
                        outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheet.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
                    }
                }



                if (outputSheet.CreatedbyUserGuid != null)
                {
                    var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.CreatedbyUserGuid).FirstOrDefaultAsync();

                    outputSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
                }

                if (outputSheet.FirstapprovalbyUserGuid != null)
                {
                    var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.FirstapprovalbyUserGuid).FirstOrDefaultAsync();

                    outputSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
                }

                if (outputSheet.SecondapprovalbyUserGuid != null)
                {
                    var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.SecondapprovalbyUserGuid).FirstOrDefaultAsync();

                    outputSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
                }


                return Ok(outputSheet);
            }
            return Unauthorized();

        }


        // GET: api/CharacterSheets/5/fullwithallitems
        /// <summary>
        /// Produces the character sheet requested by Guid along with all linked items in the JSON
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Character Sheet with All linked Items</returns>
        [HttpGet("{guid}/withallitems")]
        [Authorize]
        public async Task<ActionResult<CharacterSheet>> GetApprovedCharacterSheetWithAllItems(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheetApproved.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                if (!allowedSheets.Contains(guid))
                {
                    return Unauthorized();
                }

                var characterSheet = await _context.CharacterSheetApproved.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


                CharSheet outputSheet = Character.CreateCharSheet(characterSheet);

                var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid).FirstOrDefault();

                outputSheet.SeriesTitle = assocSeries.Title;


                var sheet_item_guid = _context.CharacterSheetApproved.Where(c => c.Isactive == true && c.Guid == guid).FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


                if (sheet_item_guid != null)
                {

                    if (_context.ItemSheetApproved.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault() != null)
                    {
                        outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproved.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());

                    }

                    else if (_context.ItemSheet.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault() != null)
                    {
                        outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheet.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
                    }
                }

                List<IteSheet> Start_Items = new List<IteSheet>();

                var StartIguids = outputSheet.Fields["Starting_Items"].ToList();

                foreach (var iGuid in StartIguids)
                {
                    if (_context.ItemSheetApproved.Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault() != null)
                    {

                        var starting_I = await _context.ItemSheetApproved.Where(issh => issh.Isactive == true &&
                         issh.Guid.ToString() == iGuid.ToString()).FirstOrDefaultAsync();


                        Start_Items.Add(Item.CreateItem(starting_I));


                    }
                    else if (_context.ItemSheet.Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault() != null)
                    {
                        //Start_Items.Add(JObject.Parse(_context.ItemSheet.Where(isa => isa.Isactive == true
                        //&& isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault().Fields.RootElement.ToString()));

                        var starting_I = await _context.ItemSheet.Where(issh => issh.Isactive == true &&
                         issh.Guid.ToString() == iGuid.ToString()).FirstOrDefaultAsync();

                        Start_Items.Add(Item.CreateItem(starting_I));
                    }

                }

                if (Start_Items != null)
                {
                    outputSheet.Starting_Items = Start_Items;
                }

                if (outputSheet.CreatedbyUserGuid != null)
                {
                    var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.CreatedbyUserGuid).FirstOrDefaultAsync();

                    outputSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
                }

                if (outputSheet.FirstapprovalbyUserGuid != null)
                {
                    var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.FirstapprovalbyUserGuid).FirstOrDefaultAsync();

                    outputSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
                }

                if (outputSheet.SecondapprovalbyUserGuid != null)
                {
                    var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.SecondapprovalbyUserGuid).FirstOrDefaultAsync();

                    outputSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
                }



                return Ok(outputSheet);
            }
            return Unauthorized();

        }



        /// <summary>
        /// Update an approved sheet WARNING WIZARD ONLY, this is to fix issues with approved sheets directly. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="characterSheetApproved"></param>
        /// <returns></returns>
        // PUT: api/CharacterSheetApproveds/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacterSheetApproved(int id, CharacterSheetApproved characterSheetApproved)
        {

            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {


                if (id != characterSheetApproved.Id)
                {
                    return BadRequest();
                }

                _context.Entry(characterSheetApproved).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterSheetApprovedExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            return Unauthorized();
        }

        /// <summary>
        /// POST new Approved sheet  WARNING: TO BE USED BY WIZARDS ONLY IN ORDER TO TRY AND REPLACE A DELETED ROW.
        /// </summary>
        /// <param name="characterSheetApproved"></param>
        /// <returns></returns>
        // POST: api/CharacterSheetApproveds
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<CharacterSheetApproved>> PostCharacterSheetApproved(CharacterSheetApproved characterSheetApproved)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                characterSheetApproved.CreatedbyuserGuid = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();

                _context.CharacterSheetApproved.Add(characterSheetApproved);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCharacterSheetApproved", new { id = characterSheetApproved.Id }, characterSheetApproved);
            }
            return Unauthorized();
        
        }

        /// <summary>
        /// Delete row from approved documents, to be used by Wizards ONLY.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/CharacterSheetApproveds/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CharacterSheetApproved>> DeleteCharacterSheetApproved(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                var characterSheetApproved = await _context.CharacterSheetApproved.FindAsync(id);
                if (characterSheetApproved == null)
                {
                    return NotFound();
                }

                _context.CharacterSheetApproved.Remove(characterSheetApproved);
                await _context.SaveChangesAsync();

                return characterSheetApproved;
            }
            return Unauthorized();

        }

        private bool CharacterSheetApprovedExists(int id)
        {
            return _context.CharacterSheetApproved.Any(e => e.Id == id);
        }
    }
}
