using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CharacterSheetsController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public CharacterSheetsController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Gets list of all active character sheets which exist in the edits table.
    /// </summary>
    /// <returns></returns>
    // GET: api/CharacterSheets
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheet()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var tagDictionary = TagScanner.getAllTagsLists(legalsheets);

            var fulltaglist = await _context.Tags.Where(t => t.Isactive == true).ToListAsync();

            var ret = await _context.CharacterSheets.Where(cs => cs.Isactive == true && allowedSheets.Contains(cs.Guid))
                .Select(c => new { c.Guid, c.Name, c.Seriesguid, c.Series.Title, c.EditbyUserGuid,
                    Createdbyuser = c.Createdbyuser.Preferredname,
                    c.FirstapprovalbyuserGuid,
                    Firstapprovalbyuser =(c.Firstapprovalbyuser.Preferredname == null || c.Firstapprovalbyuser.Preferredname == string.Empty) ? c.Firstapprovalbyuser.Firstname : c.Firstapprovalbyuser.Preferredname,
                    c.SecondapprovalbyuserGuid,
                    SecondApprovaluser = (c.Secondapprovalbyuser.Preferredname == null || c.Secondapprovalbyuser.Preferredname == string.Empty) ? c.Secondapprovalbyuser.Firstname : c.Secondapprovalbyuser.Preferredname,
                    EditbyUser = (c.EditbyUser.Preferredname == null || c.EditbyUser.Preferredname == string.Empty) ? c.EditbyUser.Firstname : c.EditbyUser.Preferredname,
                    CreatedBy = c.Createdbyuser.Preferredname,
                    Tags = TagScanner.ReturnDictElementOrNull(c.Guid, tagDictionary, fulltaglist) })
                .OrderBy(x => x.Title).ThenBy(x => x.Name).ToListAsync();

            if (ret == null) return NotFound();

            return Ok(ret);
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Gets list of all character sheets which exist in the edits table, including diabled sheets
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/CharacterSheets/IncludeDeactive
    [HttpGet("IncludeDeactive")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetWithDisabled()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var legalsheets = _context.CharacterSheets
            .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();

            var tagDictionary = TagScanner.getAllTagsLists(legalsheets);

            var fulltaglist = await _context.Tags.Where(t => t.Isactive == true).ToListAsync();

            var ret = await _context.CharacterSheets
                .Select(c => new { c.Guid, c.Name, c.Seriesguid, c.Series.Title,
                    c.EditbyUserGuid,
                    Createdbyuser = c.Createdbyuser.Preferredname,
                    c.FirstapprovalbyuserGuid,
                    Firstapprovalbyuser = (c.Firstapprovalbyuser.Preferredname == null || c.Firstapprovalbyuser.Preferredname == string.Empty) ? c.Firstapprovalbyuser.Firstname : c.Firstapprovalbyuser.Preferredname,
                    c.SecondapprovalbyuserGuid,
                    SecondApprovaluser = (c.Secondapprovalbyuser.Preferredname == null || c.Secondapprovalbyuser.Preferredname == string.Empty) ? c.Secondapprovalbyuser.Firstname : c.Secondapprovalbyuser.Preferredname,
                    EditbyUser = (c.EditbyUser.Preferredname == null || c.EditbyUser.Preferredname == string.Empty) ? c.EditbyUser.Firstname : c.EditbyUser.Preferredname,
                    CreatedBy = c.Createdbyuser.Preferredname,
                    Tags = TagScanner.ReturnDictElementOrNull(c.Guid, tagDictionary, fulltaglist) })
                .OrderBy(x => x.Title).ThenBy(x => x.Name).ToListAsync();

            return Ok(ret);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Returns full character sheet of a character by their guid.
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedSheets.Contains(guid)) return Unauthorized();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var characterSheet = await _context.CharacterSheets.Where(cs => cs.Guid == guid && cs.Isactive == true)
                .FirstOrDefaultAsync();

            if (characterSheet == null) return NotFound();


            var outputSheet = Character.CreateCharSheet(characterSheet, _context);

            var tagslist = new JsonElement();

            characterSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

            if (tagslist.ValueKind.ToString() != "Undefined")
            {
                var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                foreach (var tag in TestJsonFeilds)
                {
                    var fullTag = await _context.Tags
                        .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                    outputSheet.Tags.Add(fullTag);
                }
            }

            if (outputSheet.Img1 != null)
                if (System.IO.File.Exists(@"./images/characters/UnApproved/" + outputSheet.Img1))
                    outputSheet.imagedata1 = System.IO.File.ReadAllBytes(@"./images/characters/UnApproved/" + outputSheet.Img1);

            if (outputSheet.Img2 != null)
                if (System.IO.File.Exists(@"./images/characters/UnApproved/" + outputSheet.Img2))
                    outputSheet.imagedata2 = System.IO.File.ReadAllBytes(@"./images/characters/UnApproved/" + outputSheet.Img2);

            outputSheet.SeriesTitle = await _context.Series.Where(s => s.Guid == outputSheet.Seriesguid && s.Isactive == true)
                .Select(s => s.Title).FirstOrDefaultAsync();

            var sheet_item_guid = _context.CharacterSheets.Where(c => c.Isactive == true && c.Guid == guid)
               .FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


            if (sheet_item_guid != null) {
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                        .FirstOrDefault() != null)
                {
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
                    if (outputSheet.Sheet_Item.Img1 != null)
                        if (System.IO.File.Exists(@"./images/items/Approved/" + outputSheet.Sheet_Item.Img1))
                            outputSheet.Sheet_Item.imagedata =
                                System.IO.File.ReadAllBytes(@"./images/items/Approved/" + outputSheet.Sheet_Item.Img1);

                }

                else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                             .FirstOrDefault() !=
                         null)
                {
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheets
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
                    if (outputSheet.Sheet_Item.Img1 != null)
                        if (System.IO.File.Exists(@"./images/items/UnApproved/" + outputSheet.Sheet_Item.Img1))
                            outputSheet.Sheet_Item.imagedata =
                                System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + outputSheet.Sheet_Item.Img1);

                }
            }

            var Start_Items = new List<IteSheet>();

            var StartIguids = outputSheet.Fields["Starting_Items"].ToList();

            foreach (var iGuid in StartIguids)
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefault() != null) {
                    var starting_I = Item.CreateItem(await _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                        issh.Guid.ToString() == iGuid.ToString()).FirstOrDefaultAsync());

                    if (starting_I.Img1 != null)
                        if (System.IO.File.Exists(@"./images/items/Approved/" + starting_I.Img1))
                            starting_I.imagedata =
                                System.IO.File.ReadAllBytes(@"./images/items/Approved/" + starting_I.Img1);


                    Start_Items.Add(starting_I);
                }
                else if (_context.ItemSheets
                             .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                             .FirstOrDefault() != null) {
                    //Start_Items.Add(JObject.Parse(_context.ItemSheet.Where(isa => isa.Isactive == true
                    //&& isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault().Fields.RootElement.ToString()));
                    var starting_I = Item.CreateItem(await _context.ItemSheets.Where(issh => issh.Isactive == true &&
                                                                            issh.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefaultAsync());

                    if (starting_I.Img1 != null)
                        if (System.IO.File.Exists(@"./images/items/UnApproved/" + starting_I.Img1))
                            starting_I.imagedata =
                                System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + starting_I.Img1);

                    Start_Items.Add(starting_I);
                }

            if (Start_Items != null) outputSheet.Starting_Items = Start_Items;


            return Ok(outputSheet);
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Returns all characters from a series.
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    // GET: api/vi/CharacterSheets/BySeries
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
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var legalseries = _context.Series.Where(it => it.Isactive == true)
    .Select(it => new TagScanContainer(it.Guid, it.Tags)).ToList();
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSeries = TagScanner.ScanTags(legalseries, allowedTags);
            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedSeries.Contains(guid)) return Unauthorized();

            var tagDictionary = TagScanner.getAllTagsLists(legalsheets);

            var fulltaglist = await _context.Tags.Where(t => t.Isactive == true).ToListAsync();

            var characterSheet = await _context.CharacterSheets.Where(cs =>
                    cs.Isactive == true && cs.Seriesguid == guid && allowedSheets.Contains(cs.Guid))
                .Select(c => new {
                    c.Guid,
                    c.Name,
                    c.Seriesguid,
                    c.Series.Title,
                    CreatedBy = c.Createdbyuser.Preferredname,
                    Firstapprovalby = c.Firstapprovalbyuser.Preferredname,
                    Tags = TagScanner.ReturnDictElementOrNull(c.Guid, tagDictionary, fulltaglist)
                })
                .OrderBy(x => x.Name).ToListAsync();

            if (characterSheet == null) return NotFound();

            return Ok(characterSheet);
        }

        return Unauthorized();
    }

    [HttpGet("ByAbility")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetCharacterSheetByAbilites(
        [FromBody] JsonElement input)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var json = JsonSerializer.Serialize(input);

            using var dynJson = JsonDocument.Parse(json);

            var chars = await _context.CharacterSheets.Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid))
                .ToListAsync();

            foreach (var jo in dynJson.RootElement.EnumerateObject().GetEnumerator())
                chars = chars.Where(ch => ch.Fields.RootElement.GetProperty(jo.Name).ToString() == jo.Value.ToString())
                    .ToList();

            var output = chars.Select(cha => new
            {
                cha.Guid,
                cha.Name,
                cha.Seriesguid,
                cha.Series.Title,
                cha.Img1,
                CreatedBy = cha.Createdbyuser.Preferredname,
                Firstapprovalby = cha.Firstapprovalbyuser.Preferredname,
                cha.Fields,
                Tags = new List<Tag>()
            }).ToList();


            foreach (var sheet in output)
            {
                var tagslist = new JsonElement();

                sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        var fullTag = await _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .FirstOrDefaultAsync();
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var chars = await _context.CharacterSheets.Where(c =>
                c.Isactive == true && allowedSheets.Contains(c.Guid) && c.Fields.RootElement
                    .GetProperty("Tags").GetString().Contains(guid.ToString())).ToListAsync();


            var output = chars.Select(cha => new
            {
                cha.Guid,
                cha.Name,
                cha.Seriesguid,
                cha.Series.Title,
                cha.Img1,
                cha.Fields,
                CreatedBy = cha.Createdbyuser.Preferredname,
                Firstapprovalby = cha.Firstapprovalbyuser.Preferredname,
                Tags = new List<Tag>()
            }).ToList();

            foreach (var sheet in output) {
                var tagslist = new JsonElement();

                sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined") {
                    var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds) {
                        var fullTag = await _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .FirstOrDefaultAsync();
                        sheet.Tags.Add(fullTag);
                    }
                }
            }

            return Ok(output);
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Search for character based on partial character name or alternate name.
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var testChar = await _context.CharacterSheets
                .Where(ch => ch.Isactive == true && allowedSheets.Contains(ch.Guid)).Select(cha => new
                {
                    cha.Guid,
                    cha.Name,
                    cha.Seriesguid,
                    cha.Series.Title,
                    cha.Fields,
                    Tags = new List<Tag>()
                }).ToListAsync();

            var testQuery = testChar.Where(tc =>
                tc.Fields.RootElement.GetProperty("Alternate_Names").ToString().ToLower().Contains(input.ToLower()) ||
                tc.Name.ToLower().Contains(input.ToLower())).ToList();

            var characters = testQuery.Select(ch => new
            {
                ch.Guid,
                ch.Name,
                ch.Seriesguid,
                ch.Title,
                ch.Fields,
                ch.Tags
            }).ToList();

            if (characters == null) return NotFound();

            foreach (var sheet in characters)
            {
                var tagslist = new JsonElement();

                sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        var fullTag = await _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .FirstOrDefaultAsync();
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var allFound = new List<Guid>();

            var allSheets = await _context.CharacterSheets
                .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

            foreach (var sheet in allSheets)
            {
                var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Special_Skills").EnumerateArray();


                foreach (var Testing in TestJsonFeilds)
                {
                    var tagresult = Testing.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in tagresult)
                        if (tag.ToString() == guid.ToString())
                            allFound.Add(sheet.Guid);
                }
            }


            var output = await _context.CharacterSheets.Where(c => c.Isactive == true &&
                                                                  allFound.Contains(c.Guid)).Select(ch => new
            {
                ch.Name,
                ch.Guid,
                ch.Seriesguid,
                SeriesTitle = _context.Series.Where(s => s.Isactive == true && s.Guid == ch.Seriesguid).FirstOrDefault()
                    .Title
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var allFound = new List<Guid>();

            if (!allowedSheets.Contains(guid)) return Unauthorized();


            var characterSheet = await _context.CharacterSheets.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


            var outputSheet = Character.CreateCharSheet(characterSheet, _context);

            var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid)
                .FirstOrDefault();

            outputSheet.SeriesTitle = assocSeries.Title;


            var sheet_item_guid = _context.CharacterSheets.Where(c => c.Isactive == true && c.Guid == guid)
                .FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


            if (sheet_item_guid != null)
            {
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                        .FirstOrDefault() != null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());

                else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                             .FirstOrDefault() !=
                         null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheets
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
            }


            if (outputSheet.CreatedbyUserGuid != null)
            {
                var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.CreatedbyUserGuid)
                    .FirstOrDefaultAsync();

                outputSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }

            if (outputSheet.FirstapprovalbyUserGuid != null)
            {
                var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.FirstapprovalbyUserGuid)
                    .FirstOrDefaultAsync();

                outputSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }

            if (outputSheet.SecondapprovalbyUserGuid != null)
            {
                var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.SecondapprovalbyUserGuid)
                    .FirstOrDefaultAsync();

                outputSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }


            return Ok(outputSheet);
        }

        return Unauthorized();
    }

    // GET: api/CharacterSheets/5/fullwithallitems
    /// <summary>
    ///     Produces the character sheet requested by Guid along with all linked items in the JSON
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedSheets.Contains(guid)) return Unauthorized();

            var characterSheet = await _context.CharacterSheets.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


            var outputSheet = Character.CreateCharSheet(characterSheet, _context);

            var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid)
                .FirstOrDefault();

            outputSheet.SeriesTitle = assocSeries.Title;


            var sheet_item_guid = _context.CharacterSheets.Where(c => c.Isactive == true && c.Guid == guid)
                .FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


            if (sheet_item_guid != null)
            {
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                        .FirstOrDefault() != null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());

                else if (_context.ItemSheets.Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                             .FirstOrDefault() !=
                         null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheets
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true).FirstOrDefault());
            }

            var Start_Items = new List<IteSheet>();

            var StartIguids = outputSheet.Fields["Starting_Items"].ToList();

            foreach (var iGuid in StartIguids)
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefault() != null)
                {
                    var starting_I = await _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                        issh.Guid.ToString() == iGuid.ToString()).FirstOrDefaultAsync();


                    Start_Items.Add(Item.CreateItem(starting_I));
                }
                else if (_context.ItemSheets
                             .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                             .FirstOrDefault() != null)
                {
                    //Start_Items.Add(JObject.Parse(_context.ItemSheet.Where(isa => isa.Isactive == true
                    //&& isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault().Fields.RootElement.ToString()));
                    var starting_I = await _context.ItemSheets.Where(issh => issh.Isactive == true &&
                                                                            issh.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefaultAsync();

                    Start_Items.Add(Item.CreateItem(starting_I));
                }

            if (Start_Items != null) outputSheet.Starting_Items = Start_Items;

            if (outputSheet.CreatedbyUserGuid != null)
            {
                var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.CreatedbyUserGuid)
                    .FirstOrDefaultAsync();

                outputSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }

            if (outputSheet.FirstapprovalbyUserGuid != null)
            {
                var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.FirstapprovalbyUserGuid)
                    .FirstOrDefaultAsync();

                outputSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }

            if (outputSheet.SecondapprovalbyUserGuid != null)
            {
                var lookupuser = await _context.Users.Where(u => u.Guid == outputSheet.SecondapprovalbyUserGuid)
                    .FirstOrDefaultAsync();

                outputSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }


            return Ok(outputSheet);
        }

        return Unauthorized();
    }


    // PUT: api/CharacterSheets/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize]
    public async Task<IActionResult> PutCharacterSheet(Guid guid, CharSheet charSheet)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            // This obviously needs work when we get to characters.
            if (guid != charSheet.Guid) return BadRequest();

            if (charSheet.Img1 != null && charSheet.imagedata1 != null && charSheet.imagedata1.Length != 0) {

                var folderName = Path.Combine("images", "characters", "UnApproved");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (charSheet.imagedata1.Length > 0) {
                    if (!Directory.Exists(pathToSave + "/")) {
                        var di = Directory.CreateDirectory(pathToSave + "/");
                    }
                    System.IO.File.WriteAllBytes(pathToSave + "/" + charSheet.Img1, charSheet.imagedata1);
                }
            }
            if (charSheet.Img2 != null && charSheet.imagedata2 != null && charSheet.imagedata2.Length != 0) {;

                var folderName = Path.Combine("images", "characters", "UnApproved");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (charSheet.imagedata2.Length > 0) {
                    if (!Directory.Exists(pathToSave + "/")) {
                        var di = Directory.CreateDirectory(pathToSave + "/");
                    }
                    System.IO.File.WriteAllBytes(pathToSave + "/" + charSheet.Img2, charSheet.imagedata2);
                }
            }

            CharacterSheet characterSheet = charSheet.OutputToCharacterSheet();

            characterSheet.Createdate = DateTime.UtcNow;
            characterSheet.EditbyUserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            characterSheet.CreatedbyuserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            characterSheet.FirstapprovalbyuserGuid = null;
            characterSheet.Firstapprovaldate = null;
            characterSheet.SecondapprovalbyuserGuid = null;
            characterSheet.Secondapprovaldate = null;
            characterSheet.Isactive = true;
            characterSheet.Version = 1;

            var charsheets = await _context.CharacterSheets.Where(cs => cs.Guid == characterSheet.Guid).ToListAsync();
            if (charsheets != null && charsheets.Count > 0)
            {
                var maxsheet = charsheets.MaxBy(csa => csa.Version);
                characterSheet.Version = maxsheet.Version;
                characterSheet.CreatedbyuserGuid = maxsheet.CreatedbyuserGuid;
                characterSheet.Version++;
            }

            foreach (var sheet in charsheets) {
                sheet.Isactive = false;
                _context.Update(sheet);

                try {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!CharacterSheetExists(guid))
                        return NotFound();
                    throw;
                }
            }

            _context.CharacterSheets.Add(characterSheet);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacterSheetExists(guid))
                    return NotFound();
                throw;
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
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedSheets.Contains(guid)) return Unauthorized();

            var characterSheet = await _context.CharacterSheets.Where(cs => cs.Isactive == true && cs.Guid == guid)
                .FirstOrDefaultAsync();
            var fullapprove = false;

            var result = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();

            if (characterSheet == null) return BadRequest();

            if (characterSheet.FirstapprovalbyuserGuid != null && characterSheet.SecondapprovalbyuserGuid == null)
            {
                if (result != characterSheet.EditbyUserGuid && result != characterSheet.FirstapprovalbyuserGuid)
                {
                    characterSheet.SecondapprovalbyuserGuid = result;
                    characterSheet.Secondapprovaldate = DateTime.UtcNow;
                    characterSheet.Isactive = false;
                    fullapprove = true;
                }
                else
                {
                    return Unauthorized();
                }
            }


            if (characterSheet.FirstapprovalbyuserGuid == null)
            {
                if (result != characterSheet.EditbyUserGuid)
                {
                    //characterSheet.SecondapprovalbyuserGuid = null;
                    characterSheet.FirstapprovalbyuserGuid = result;
                    characterSheet.Firstapprovaldate = DateTime.UtcNow;
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
                    return NotFound();
                throw;
            }

            if (fullapprove)
            {
                var approvedSheets = await _context.CharacterSheetApproveds.Where(csa => csa.Guid == guid).ToListAsync();

                var maxversion = 0;

                foreach (var asheet in approvedSheets)
                {
                    if (maxversion < asheet.Version) maxversion = asheet.Version;

                    asheet.Isactive = false;

                    _context.CharacterSheetApproveds.Update(asheet);
                    await _context.SaveChangesAsync();
                }

                maxversion++;

                CharacterSheetApproved theNewSheet = new CharacterSheetApproved()
                {
                     CharactersheetId = characterSheet.Id,
                      Guid = characterSheet.Guid,
                    Seriesguid =  characterSheet.Seriesguid,
                    Name = characterSheet.Name,
                    Img1 = characterSheet.Img1,
                    Img2 = characterSheet.Img2,
                    Fields = characterSheet.Fields,
                    Isactive = true,
                    Createdate = characterSheet.Createdate,
                    CreatedbyuserGuid = characterSheet.EditbyUserGuid,
                    FirstapprovalbyuserGuid = characterSheet.FirstapprovalbyuserGuid,
                    Firstapprovaldate = characterSheet.Firstapprovaldate,
                    SecondapprovalbyuserGuid = characterSheet.SecondapprovalbyuserGuid,
                    Secondapprovaldate = characterSheet.Secondapprovaldate,
                    Gmnotes = characterSheet.Gmnotes,
                    Reason4edit = characterSheet.Reason4edit,
                    Version = characterSheet.Version
                };

                if (theNewSheet.Img1 != null)
                {

                    var folderName = Path.Combine("images", "characters", "UnApproved");
                    var pathCopyFrom = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    var toFolderName = Path.Combine("images", "characters", "Approved");
                    var pathCopyTo = Path.Combine(Directory.GetCurrentDirectory(), toFolderName);

                    if (!Directory.Exists(pathCopyTo + "/"))
                    {
                        var di = Directory.CreateDirectory(pathCopyTo + "/");
                    }

                    System.IO.File.Copy(pathCopyFrom + "/" + theNewSheet.Img1, pathCopyTo + "/" + theNewSheet.Img1, true);
                }
                if (theNewSheet.Img2 != null)
                {

                    var folderName = Path.Combine("images", "characters", "UnApproved");
                    var pathCopyFrom = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    var toFolderName = Path.Combine("images", "characters", "Approved");
                    var pathCopyTo = Path.Combine(Directory.GetCurrentDirectory(), toFolderName);

                    if (!Directory.Exists(pathCopyTo + "/"))
                    {
                        var di = Directory.CreateDirectory(pathCopyTo + "/");
                    }

                    System.IO.File.Copy(pathCopyFrom + "/" + theNewSheet.Img2, pathCopyTo + "/" + theNewSheet.Img2, true);
                }


                // theNewSheet.Isactive = true;
                // theNewSheet.SecondapprovalbyuserGuid = result;
                // theNewSheet.Secondapprovaldate = DateTime.UtcNow;

                _context.CharacterSheetApproveds.Add(theNewSheet);
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
    public async Task<ActionResult<CharSheet>> PostCharacterSheet([FromBody] CharSheet charSheet)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var characterSheet = new CharacterSheet();

            if (charSheet.Guid != null) characterSheet.Guid = charSheet.Guid;

            if (charSheet.Name != null) characterSheet.Name = charSheet.Name;

            if (charSheet.Img1 != null) characterSheet.Img1 = charSheet.Img1;

            if (charSheet.Img2 != null) characterSheet.Img2 = charSheet.Img2;

            if (charSheet.Img1 != null && charSheet.imagedata1 != null && charSheet.imagedata1.Length != 0) {
                charSheet.Img1 = charSheet.Img1;

                var folderName = Path.Combine("images", "characters", "UnApproved");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (charSheet.imagedata1.Length > 0) {
                    if (!Directory.Exists(pathToSave + "/")) {
                        var di = Directory.CreateDirectory(pathToSave + "/");
                    }

                    System.IO.File.WriteAllBytes(pathToSave + "/" + charSheet.Img1, charSheet.imagedata1);
                }
            }

            if (charSheet.Img2 != null && charSheet.imagedata2 != null && charSheet.imagedata2.Length != 0) {
                charSheet.Img2 = charSheet.Img2;

                var folderName = Path.Combine("images", "characters", "UnApproved");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (charSheet.imagedata2.Length > 0) {
                    if (!Directory.Exists(pathToSave + "/")) {
                        var di = Directory.CreateDirectory(pathToSave + "/");
                    }

                    System.IO.File.WriteAllBytes(pathToSave + "/" + charSheet.Img2, charSheet.imagedata2);
                }
            }

            if (charSheet.Gmnotes != null) characterSheet.Gmnotes = charSheet.Gmnotes;

            if (charSheet.Fields != null)
            {
                foreach (var tag in charSheet.Fields)
                    if (tag.Key == "Tags")
                    {
                        var TestJsonFeilds = charSheet.Fields["Tags"];

                        foreach (Guid tagValue in TestJsonFeilds)
                            if (!allowedTags.Contains(tagValue))
                                return Unauthorized();
                    }

                characterSheet.Fields = JsonDocument.Parse(charSheet.Fields.ToString());
            }

            if (charSheet.Reason4edit != null) characterSheet.Reason4edit = charSheet.Reason4edit;

            if (charSheet.Seriesguid != null) characterSheet.Seriesguid = (Guid)charSheet.Seriesguid;


            characterSheet.Createdate = DateTime.UtcNow;
            characterSheet.CreatedbyuserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            characterSheet.EditbyUserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            characterSheet.FirstapprovalbyuserGuid = null;
            characterSheet.Firstapprovaldate = null;
            characterSheet.SecondapprovalbyuserGuid = null;
            characterSheet.Secondapprovaldate = null;
            characterSheet.Isactive = true;

            var approvedSheets = await _context.CharacterSheets.Where(csa => csa.Guid == characterSheet.Guid).ToListAsync();

            if (approvedSheets != null && approvedSheets.Count > 0)
            {
                var maxsheet = approvedSheets.MaxBy(csa => csa.Version);
                charSheet.Version = maxsheet.Version++;
            }

            _context.CharacterSheets.Add(characterSheet);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCharacterSheet", new { id = characterSheet.Guid }, charSheet);
        }

        return Unauthorized();
    }

    // DELETE: api/CharacterSheets/5
    [HttpDelete("{guid}")]
    public async Task<ActionResult<CharacterSheet>> DeleteCharacterSheet(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var characterSheets = await _context.CharacterSheets.Where(csa => csa.Guid == guid).ToListAsync();
            if (characterSheets == null) return NotFound();

            foreach (var character in characterSheets)
            {
                if (character.Isactive == true)
                {
                    character.Isactive = false;
                    _context.CharacterSheets.Update(character);
                    await _context.SaveChangesAsync();
                }
            }

            return characterSheets.FirstOrDefault();
        }

        return Unauthorized();
    }

    private bool CharacterSheetExists(Guid id)
    {
        return _context.CharacterSheets.Any(e => e.Guid == id);
    }

}