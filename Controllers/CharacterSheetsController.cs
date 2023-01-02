using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using NEXUSDataLayerScaffold.Entities;
using Microsoft.AspNetCore.Authentication;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CharacterSheetsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public CharacterSheetsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets list of all active character sheets which exist in the edits table.
        /// </summary>
        /// <returns></returns>
        // GET: api/CharacterSheets
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheet([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                var ret = await _context.CharacterSheet.Where(cs => cs.Isactive == true  && allowedSheets.Contains(cs.Guid)).Select(c => new { c.Guid, c.Name, c.Seriesguid, c.Seriesgu.Title })
                    .OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                return Ok(ret);
            }
            return Unauthorized();

        }


        /// <summary>
        /// Gets list of all character sheets which exist in the edits table, including diabled sheets
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/CharacterSheets/IncludeDeactive
        [HttpGet("IncludeDeactive")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetWithDisabled([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                var ret = await _context.CharacterSheet.Select(c => new { c.Guid, c.Name, c.Isactive })
                    .OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                return Ok(ret);
            }
            return Unauthorized();

        }

        /// <summary>
        /// Returns full character sheet of a character by their guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        // GET: api/vi/CharacterSheets/{guid}
        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<CharacterSheet>> GetCharacterSheet(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                if(!allowedSheets.Contains(guid))
                {
                    return Unauthorized();
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var characterSheet = await _context.CharacterSheet.Where(cs => cs.Guid == guid && cs.Isactive == true).FirstOrDefaultAsync();

                if (characterSheet == null)
                {
                    return NotFound();
                }

               


                CharSheet outputSheet = Character.CreateCharSheet(characterSheet);

                JsonElement tagslist = new JsonElement();

                characterSheet.Fields.RootElement.TryGetProperty("Character_Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Character_Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                        outputSheet.Tags.Add(fullTag);
                    }
                }


                return Ok(outputSheet);
            }
            return Unauthorized();
        }


        /// <summary>
        /// Returns all characters from a series.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        // GET: api/vi/CharacterSheets/BySeries
        [HttpGet("BySeries")]
        [Authorize]
        public async Task<ActionResult<CharacterSheet>> GetCharacterSheetBySeries([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                var characterSheets = await _context.CharacterSheet.Where(cs => cs.Isactive == true && allowedSheets.Contains(cs.Guid) && cs.Seriesguid== pagingParameterModel.guid)
                    .OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                if (characterSheets == null)
                {
                    return NotFound();
                }

                List<CharSheet> outPutCharacterList = new List<CharSheet>();

                foreach (var sheet in characterSheets)
                {
                    var outPutSheet = Extensions.Character.CreateCharSheet(sheet);

                    JsonElement tagslist = new JsonElement();

                    sheet.Fields.RootElement.TryGetProperty("Character_Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Character_Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                            outPutSheet.Tags.Add(fullTag);
                        }
                    }

                    outPutCharacterList.Add(outPutSheet);
                }

                return Ok(outPutCharacterList.OrderBy(x => x.Name));
            }
            return Unauthorized();
        }

        [HttpGet("ByAbility")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetByAbilites([FromBody] JsonElement input)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
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

                var chars = await _context.CharacterSheet.Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

                foreach (var jo in dynJson.RootElement.EnumerateObject().GetEnumerator())
                {
                    chars = chars.Where(ch => ch.Fields.RootElement.GetProperty(jo.Name.ToString()).ToString() == jo.Value.ToString()).ToList();

                }

                var output = chars.Select(cha => new { 
                    cha.Guid, 
                    cha.Name, 
                    cha.Seriesguid, 
                    cha.Seriesgu.Title,
                    cha.Img1,
                    cha.Fields,
                    Tags = new List<Tags>(),
            }).ToList();



                foreach (var sheet in output)
                {
                    JsonElement tagslist = new JsonElement();

                    sheet.Fields.RootElement.TryGetProperty("Character_Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Character_Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                            sheet.Tags.Add(fullTag);
                        }
                    }

                }

                return Ok(output);
            }
            return Unauthorized();
        }


        [HttpGet("ByTag/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetByTag(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                var chars = await _context.CharacterSheet.Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid) && c.Fields.RootElement.GetProperty("Character_Tags").GetString().Contains(guid.ToString())).ToListAsync();


                var output = chars.Select(cha => new
                {
                    cha.Guid,
                    cha.Name,
                    cha.Seriesguid,
                    cha.Seriesgu.Title,
                    cha.Img1,
                    cha.Fields,
                    Tags = new List<Tags>(),
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
        public async Task<ActionResult<Series>> GetCharacterSearchPartial(string input)
        {

            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                var testChar = await _context.CharacterSheet.Where(ch => ch.Isactive == true  && allowedSheets.Contains(ch.Guid)).Select(cha => new {
                    cha.Guid,
                    cha.Name,
                    cha.Seriesguid,
                    cha.Seriesgu.Title,
                    cha.Fields,
                  Tags = new List<Tags>(),
                }).ToListAsync();

                var testQuery = testChar.Where(tc => tc.Fields.RootElement.GetProperty("Alternate_Names").ToString().ToLower().Contains(input.ToLower()) ||
                tc.Name.ToLower().Contains(input.ToLower())).ToList();

                var characters = testQuery.Select(ch => new { 
                ch.Guid,
                ch.Name,
                ch.Seriesguid,
                ch.Title,
                    ch.Fields,
                    ch.Tags
                }).ToList();

                if (characters == null)
                {
                    return NotFound();
                }

                foreach (var sheet in characters)
                {
                    JsonElement tagslist = new JsonElement();

                    sheet.Fields.RootElement.TryGetProperty("Character_Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Character_Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                            sheet.Tags.Add(fullTag);
                        }
                    }

                }



                return Ok(characters);
            }
            return Unauthorized();
        }



        [HttpGet("BySpecialSkillsTag/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetBySpecialSkillsTag(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                List<Guid> allFound = new List<Guid>();

                var allSheets = await _context.CharacterSheet.Where(c => c.Isactive == true  && allowedSheets.Contains(c.Guid)).ToListAsync();

                foreach (var sheet in allSheets)
                {
                    var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Special_Skills").EnumerateArray();


                    foreach (var Testing in TestJsonFeilds)
                    {
                        var tagresult = Testing.GetProperty("Character_Tags").EnumerateArray();

                        foreach (var tag in tagresult)
                        {
                            if (tag.ToString() == guid.ToString())
                            {

                                allFound.Add(sheet.Guid);
                            }
                        }

                    }
                }



                var output = await _context.CharacterSheet.Where(c => c.Isactive == true &&
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
        public async Task<ActionResult<CharacterSheet>> GetCharacterSheetWithSheetItem(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                List<Guid> allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

                List<Guid> allFound = new List<Guid>();

                if (!allowedSheets.Contains(guid))
                {
                    return Unauthorized();
                }


                var characterSheet = await _context.CharacterSheet.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


                CharSheet outputSheet = Character.CreateCharSheet(characterSheet);

                var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid).FirstOrDefault();

                outputSheet.SeriesTitle = assocSeries.Title;


                var sheet_item_guid = _context.CharacterSheet.Where(c => c.Isactive == true && c.Guid == guid).FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


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
        public async Task<ActionResult<CharacterSheet>> GetCharacterSheetWithAllItems(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
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

                var characterSheet = await _context.CharacterSheet.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


                CharSheet outputSheet = Character.CreateCharSheet(characterSheet);

                var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid).FirstOrDefault();

                outputSheet.SeriesTitle = assocSeries.Title;


                var sheet_item_guid = _context.CharacterSheet.Where(c => c.Isactive == true && c.Guid == guid).FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


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


        // PUT: api/CharacterSheets/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCharacterSheet(Guid id, CharacterSheet characterSheet)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {

                // This obviously needs work when we get to characters.
                if (id != characterSheet.Guid)
                {
                    return BadRequest();
                }

                characterSheet.Createdate = DateTime.Now;
                characterSheet.CreatedbyuserGuid = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
                characterSheet.FirstapprovalbyuserGuid = null;
                characterSheet.Firstapprovaldate = null;
                characterSheet.SecondapprovalbyuserGuid = null;
                characterSheet.Secondapprovaldate = null;
                characterSheet.Isactive = true;

                _context.Entry(characterSheet).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterSheetExists(id))
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

        [HttpPut("{guid}/Approve")]
        [Authorize]
        public async Task<IActionResult> ApproveCharacterSheet(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Approver", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
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

                CharacterSheet characterSheet = await _context.CharacterSheet.Where(cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();
                bool fullapprove = false;

                var result = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();

                if (characterSheet == null)
                {
                    return BadRequest();
                }

                if (characterSheet.FirstapprovalbyuserGuid != null && characterSheet.SecondapprovalbyuserGuid == null)
                {
                    if (result != characterSheet.CreatedbyuserGuid && result != characterSheet.FirstapprovalbyuserGuid)
                    {
                        characterSheet.SecondapprovalbyuserGuid = result;
                        characterSheet.Secondapprovaldate= DateTime.Now;
                        characterSheet.Isactive = false;
                        fullapprove=true;
                    }
                    else
                    {
                        return Unauthorized();
                    }

                }


                if (characterSheet.FirstapprovalbyuserGuid == null)
                {
                    if (result != characterSheet.CreatedbyuserGuid)
                    {
                        //characterSheet.SecondapprovalbyuserGuid = null;
                        characterSheet.FirstapprovalbyuserGuid = result;
                        characterSheet.Firstapprovaldate = DateTime.Now;
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }


                _context.Entry(characterSheet).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterSheetExists(guid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                if (fullapprove)
                {
                    var approvedSheets = await _context.CharacterSheetApproved.Where(csa => csa.Guid==guid).ToListAsync();

                    int maxversion = 0;

                    foreach (var asheet in approvedSheets)
                    {
                        if (maxversion < asheet.Version)
                        {
                            maxversion = asheet.Version;
                        }

                        asheet.Isactive = false;

                        _context.CharacterSheetApproved.Update(asheet);
                        await _context.SaveChangesAsync();

                    }

                    var theNewSheet = await _context.CharacterSheetApproved.Where(csa => csa.Guid == guid && csa.Version == maxversion).FirstOrDefaultAsync();

                    theNewSheet.Isactive = true;
                    theNewSheet.SecondapprovalbyuserGuid = result;
                    theNewSheet.Secondapprovaldate = DateTime.Now;

                    _context.CharacterSheetApproved.Update(theNewSheet);
                    _context.SaveChanges();

                }




                return NoContent();
            }
            return Unauthorized();
        }




        // POST: api/CharacterSheets
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<CharacterSheet>> PostCharacterSheet([FromBody] CharSheet charSheet)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {

                List<TagScanContainer> legalsheets = _context.CharacterSheet.Where(it => it.Isactive == true).Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Usergu.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt => (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();
                }

                CharacterSheet characterSheet = new CharacterSheet();

                if (charSheet.Name != null)
                {
                    characterSheet.Name = charSheet.Name;
                }

                if (charSheet.Img1 != null)
                {
                    characterSheet.Img1 = charSheet.Img1;
                }

                if (charSheet.Img2 != null)
                {
                    characterSheet.Img2 = charSheet.Img2;
                }

                if (charSheet.Gmnotes != null)
                {
                    characterSheet.Gmnotes = charSheet.Gmnotes;
                }

                if (charSheet.Fields != null)
                {
                    foreach (var tag in charSheet.Fields)
                    {

                        if (tag.Key == "Tags")
                        {
                            var TestJsonFeilds = charSheet.Fields["Tags"];

                            foreach (Guid tagValue in TestJsonFeilds)
                            {
                                if (!allowedTags.Contains(tagValue))
                                {
                                    return Unauthorized();
                                }
                            }
                        }

                    }
                    characterSheet.Fields = JsonDocument.Parse(charSheet.Fields.ToString());
                }

                if (charSheet.Reason4edit != null)
                {
                    characterSheet.Reason4edit = charSheet.Reason4edit;
                }

                if (charSheet.Seriesguid != null)
                {
                    characterSheet.Seriesguid = charSheet.Seriesguid;
                }


                characterSheet.Createdate = DateTime.Now;
                characterSheet.CreatedbyuserGuid = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
                characterSheet.FirstapprovalbyuserGuid = null;
                characterSheet.Firstapprovaldate = null;
                characterSheet.SecondapprovalbyuserGuid = null;
                characterSheet.Secondapprovaldate = null;
                characterSheet.Isactive = true;


                _context.CharacterSheet.Add(characterSheet);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCharacterSheet", new { id = characterSheet.Guid }, characterSheet);
            }
            return Unauthorized();
        }

        // DELETE: api/CharacterSheets/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CharacterSheet>> DeleteCharacterSheet(Guid id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {


                var characterSheet = await _context.CharacterSheet.FindAsync(id);
                if (characterSheet == null)
                {
                    return NotFound();
                }

                _context.CharacterSheet.Remove(characterSheet);
                await _context.SaveChangesAsync();

                return characterSheet;
            }
            return Unauthorized();
        }

        private bool CharacterSheetExists(Guid id)
        {
            return _context.CharacterSheet.Any(e => e.Guid == id);
        }


    }
}
