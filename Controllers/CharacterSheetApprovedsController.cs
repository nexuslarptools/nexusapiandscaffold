using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CharacterSheetApprovedsController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public CharacterSheetApprovedsController(NexusLarpLocalContext context)
    {
        _context = context;
    }


    /// <summary>
    ///     Gets list of all active character sheets which exist in the approved characters table.
    /// </summary>
    /// <returns></returns>
    // GET: api/V1/CharacterSheetApproveds
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CharSheetListItem>>> GetCharacterSheetApproved()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var wizardauth = UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context);

            //var legalsheets = _context.CharacterSheets.Where(it => it.Isactive == true)
            //    .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles
                .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid)
                .ToList();

            var disAllowedTags = _context.Larptags.Where(lt =>
                !(allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allSheets = await _context.CharacterSheetApproveds.Where(c => c.Isactive == true)
.Select(x => new CharacterSheetApprovedDO
{
    Sheet = new CharacterSheetApproved
    {
        Guid = x.Guid,
        Seriesguid = x.Seriesguid,
        Name = x.Name,
        Createdate = x.Createdate,
        CreatedbyuserGuid = x.CreatedbyuserGuid,
        FirstapprovalbyuserGuid = x.FirstapprovalbyuserGuid,
        Firstapprovaldate = x.Firstapprovaldate,
        SecondapprovalbyuserGuid = x.SecondapprovalbyuserGuid,
        Secondapprovaldate = x.Secondapprovaldate,
        EditbyUserGuid = x.EditbyUserGuid,
        Taglists = x.Taglists
    },
    TagList = x.CharacterSheetApprovedTags.Select(cst => cst.Tag).OrderBy(cst => cst.Name).ToList(),
    Createdbyuser = x.Createdbyuser,
    EditbyUser = x.EditbyUser,
    Firstapprovalbyuser = x.Firstapprovalbyuser,
    Secondapprovalbyuser = x.Secondapprovalbyuser,
    Series = x.Series,
    CharacterSheetReviewMessages = _context.CharacterSheetReviewMessages.Where(csr => csr.CharactersheetId == x.Id).ToList()
})
                .OrderBy(x =>x.Sheet.Name).ToListAsync();

            if (!wizardauth)
                allSheets = allSheets.Where(ash => !disAllowedTags.Any(dat => ash.TagList.Any(tl => tl.Guid == dat)))
                    .ToList();

            var outp = new List<CharSheetListItem>();
            foreach (var c in allSheets)
            {
                var newshee = new CharSheetListItem(c, _context);
                outp.Add(newshee);
            }


            if (outp == null) return NotFound();

            return Ok(outp.OrderBy(o => StringLogic.IgnorePunct(o.name)).OrderBy(o => StringLogic.IgnorePunct(o.title)));
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Gets list of all character sheets which exist in the Approved table, including diabled sheets
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
            var legalsheets = _context.CharacterSheetApproveds
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();

            var tagDictionary = TagScanner.getAllTagsLists(legalsheets);

            var fullTagList = await _context.Tags.Where(t => t.Isactive == true).ToListAsync();

            var ret = await _context.CharacterSheetApproveds
                .Select(c => new
                {
                    c.Id, c.Guid, c.Name, c.Seriesguid, c.Series.Title, c.Isactive,
                    Tags = TagScanner.ReturnDictElementOrNull(c.Guid, tagDictionary, fullTagList)
                }).ToListAsync();

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
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
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

            var characterSheet = await _context.CharacterSheetApproveds
                .Where(cs => cs.Guid == guid && cs.Isactive == true).FirstOrDefaultAsync();

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
                        .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).Include("Tagtype")
                        .FirstOrDefaultAsync();
                    outputSheet.Tags.Add(new TagOut(fullTag));
                }
            }

            //if (outputSheet.Img1 != null)
            //    if (System.IO.File.Exists(@"./images/characters/Approved/" + outputSheet.Img1))
            //        outputSheet.imagedata1 =
            //            System.IO.File.ReadAllBytes(@"./images/characters/Approved/" + outputSheet.Img1);

            //if (outputSheet.Img2 != null)
            //    if (System.IO.File.Exists(@"./images/characters/Approved/" + outputSheet.Img2))
            //        outputSheet.imagedata2 =
            //            System.IO.File.ReadAllBytes(@"./images/characters/Approved/" + outputSheet.Img2);


            outputSheet.SeriesTitle = await _context.Series
                .Where(s => s.Guid == outputSheet.Seriesguid && s.Isactive == true)
                .Select(s => s.Title).FirstOrDefaultAsync();

            var sheet_item_guid = _context.CharacterSheetApproveds.Where(c => c.Isactive == true && c.Guid == guid)
                .FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();

            if (sheet_item_guid != null)
            {
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                        .FirstOrDefault() != null)
                {
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                            .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                            .FirstOrDefault(),
                        _context);
                    //if (outputSheet.Sheet_Item.Img1 != null)
                    //    if (System.IO.File.Exists(@"./images/items/Approved/" + outputSheet.Sheet_Item.Img1))
                    //        outputSheet.Sheet_Item.imagedata =
                     //           System.IO.File.ReadAllBytes(@"./images/items/Approved/" + outputSheet.Sheet_Item.Img1);
                }

                else if (_context.ItemSheets
                             .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                             .FirstOrDefault() !=
                         null)
                {
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheets
                            .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                            .FirstOrDefault(),
                        _context);
                    //if (outputSheet.Sheet_Item.Img1 != null)
                     //   if (System.IO.File.Exists(@"./images/items/UnApproved/" + outputSheet.Sheet_Item.Img1))
                     //       outputSheet.Sheet_Item.imagedata =
                     //           System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" +
                     //                                       outputSheet.Sheet_Item.Img1);
                }
            }


            var Start_Items = new List<IteSheet>();

            if (outputSheet.Sheet_Item != null && outputSheet.Sheet_Item.Islarge)
            {
                outputSheet.Sheet_Item.Issheetitem = true;
                Start_Items.Add(outputSheet.Sheet_Item);
            }

            var StartIguids = outputSheet.Fields["Starting_Items"].ToList();

            foreach (var iGuid in StartIguids)
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefault() != null)
                {
                    var starting_I = Item.CreateItem(await _context.ItemSheetApproveds.Where(issh =>
                        issh.Isactive == true &&
                        issh.Guid.ToString() == iGuid.ToString()).FirstOrDefaultAsync(), _context);

                   // if (starting_I.Img1 != null)
                    //    if (System.IO.File.Exists(@"./images/items/Approved/" + starting_I.Img1))
                    //        starting_I.imagedata =
                    //            System.IO.File.ReadAllBytes(@"./images/items/Approved/" + starting_I.Img1);

                    Start_Items.Add(starting_I);
                }
                else if (_context.ItemSheets
                             .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                             .FirstOrDefault() != null)
                {
                    //Start_Items.Add(JObject.Parse(_context.ItemSheet.Where(isa => isa.Isactive == true
                    //&& isa.Guid.ToString() == iGuid.ToString()).FirstOrDefault().Fields.RootElement.ToString()));
                    var starting_I = Item.CreateItem(await _context.ItemSheets.Where(issh => issh.Isactive == true &&
                            issh.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefaultAsync(), _context);

                   // if (starting_I.Img1 != null)
                    //    if (System.IO.File.Exists(@"./images/items/UnApproved/" + starting_I.Img1))
                    //        starting_I.imagedata =
                     //           System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + starting_I.Img1);

                    Start_Items.Add(starting_I);
                }

            if (Start_Items != null) outputSheet.Starting_Items = Start_Items;


            return Ok(outputSheet);
        }

        return Unauthorized();
    }

    // GET: api/CharacterSheetApproveds/5
    [HttpGet("MultiCharacter/{guids}")]
    [Authorize]
    public async Task<ActionResult<CharListWithItemList>> GetMultiCharacterSheetItemGrouped(string guids)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;
        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var output = new CharListWithItemList();
            var guidlist = guids.Split(',');

            foreach (var input in guidlist)
                if (Guid.TryParse(input, out var inputGuid))
                {
                    var charSheetA = _context.CharacterSheetApproveds
                        .Where(csa => csa.Isactive == true && csa.Guid == inputGuid).FirstOrDefault();
                    var charSheet = _context.CharacterSheets.Where(cs => cs.Isactive == true && cs.Guid == inputGuid)
                        .FirstOrDefault();

                    if (charSheetA == null && charSheet != null)
                    {
                        if (!UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context)) return Unauthorized();
                        var cha = new CharSheet(charSheet, _context);
                        output.Characters.Add(cha);
                        output.Items.Add(cha.Sheet_Item);
                        foreach (var sItem in cha.Starting_Items) output.Items.Add(sItem);
                    }
                    else if (charSheetA != null)
                    {
                        var cha = new CharSheet(charSheetA, _context);
                        output.Characters.Add(cha);
                        output.Items.Add(cha.Sheet_Item);
                        foreach (var sItem in cha.Starting_Items) output.Items.Add(sItem);
                    }
                }
                else
                {
                    return BadRequest();
                }

            return Ok(output);
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Returns all characters from a series.
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
            var legalseries = _context.Series.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.SeriesTags)).ToList();
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
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


            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var tagDictionary = TagScanner.getAllTagsLists(legalsheets);

            var fullTagList = await _context.Tags.Where(t => t.Isactive == true).ToListAsync();

            var characterSheet = await _context.CharacterSheetApproveds.Where(cs =>
                    cs.Isactive == true && cs.Seriesguid == guid && allowedSheets.Contains(cs.Guid))
                .Select(c => new
                {
                    c.Guid,
                    c.Name,
                    c.Seriesguid,
                    c.Series.Title,
                    Tags = TagScanner.ReturnDictElementOrNull(c.Guid, tagDictionary, fullTagList)
                }).OrderBy(x => StringLogic.IgnorePunct(x.Title)).ThenBy(x => StringLogic.IgnorePunct(x.Name)).ToListAsync();

            if (characterSheet == null) return NotFound();

            return Ok(characterSheet);
        }

        return Unauthorized();
    }


    [HttpGet("ByAbility")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetApprovedCharacterSheetByAbilites(
        [FromBody] JsonElement input)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
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

            var chars = await _context.CharacterSheetApproveds
                .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

            foreach (var jo in dynJson.RootElement.EnumerateObject().GetEnumerator())
                chars = chars.Where(ch => ch.Fields.RootElement.GetProperty(jo.Name).ToString() == jo.Value.ToString())
                    .ToList();

            var output = chars.Select(cha => new { cha.Guid, cha.Name, cha.Seriesguid, cha.Series.Title }).ToList();

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
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (!allowedTags.Contains(guid)) return Unauthorized();

            var chars = await _context.CharacterSheetApproveds.Where(c =>
                c.Isactive == true && c.Fields.RootElement.GetProperty("Tags").GetString()
                    .Contains(guid.ToString())).ToListAsync();


            var output = chars.Select(cha => new
            {
                cha.Guid,
                cha.Name,
                SeriesGuid = cha.Seriesguid,
                SeriesTitle = _context.Series.Where(s => s.Isactive == true && s.Guid == cha.Seriesguid)
                    .FirstOrDefault().Title
            }).ToList();

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
    public async Task<ActionResult<Series>> GetApprovedCharacterSearchPartial(string input)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var testChar = await _context.CharacterSheetApproveds
                .Where(ch => ch.Isactive == true && allowedSheets.Contains(ch.Guid)).Select(cha => new
                {
                    cha.Guid,
                    cha.Name,
                    cha.Seriesguid,
                    cha.Series.Title,
                    cha.Fields
                }).ToListAsync();

            var testQuery = testChar.Where(tc =>
                tc.Fields.RootElement.GetProperty("Alternate Names").ToString().ToLower().Contains(input.ToLower()) ||
                tc.Name.ToLower().Contains(input.ToLower())).ToList();


            var characters = testQuery.Select(ch => new
            {
                ch.Guid,
                ch.Name,
                ch.Seriesguid,
                ch.Title
            }).ToList();

            if (characters == null) return NotFound();


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
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);


            var allFound = new List<Guid>();

            var allSheets = await _context.CharacterSheetApproveds
                .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

            foreach (var sheet in allSheets)
                if (sheet.Fields.RootElement.TryGetProperty("Special_Skills", out var sSkilsJsonFields))
                {
                    var TestJsonFeilds = sSkilsJsonFields.EnumerateArray();

                    foreach (var Testing in TestJsonFeilds)
                    {
                        var tagresult = Testing.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in tagresult)
                            if (tag.ToString() == guid.ToString())
                                allFound.Add(sheet.Guid);
                    }
                }


            var output = await _context.CharacterSheetApproveds.Where(c => c.Isactive == true &&
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
    public async Task<ActionResult<CharacterSheet>> GetApprovedCharacterSheetWithSheetItem(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedSheets.Contains(guid)) return Unauthorized();

            var characterSheet = await _context.CharacterSheetApproveds.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


            var outputSheet = Character.CreateCharSheet(characterSheet, _context);

            var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid)
                .FirstOrDefault();

            outputSheet.SeriesTitle = assocSeries.Title;


            var sheet_item_guid = _context.CharacterSheetApproveds.Where(c => c.Isactive == true && c.Guid == guid)
                .FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


            if (sheet_item_guid != null)
            {
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                        .FirstOrDefault() != null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                            .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                            .FirstOrDefault(),
                        _context);

                else if (_context.ItemSheets
                             .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                             .FirstOrDefault() !=
                         null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheets
                            .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                            .FirstOrDefault(),
                        _context);
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
    public async Task<ActionResult<CharacterSheet>> GetApprovedCharacterSheetWithAllItems(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.CharacterSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.CharacterSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedSheets.Contains(guid)) return Unauthorized();

            var characterSheet = await _context.CharacterSheetApproveds.Where
                (cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();


            var outputSheet = Character.CreateCharSheet(characterSheet, _context);

            var assocSeries = _context.Series.Where(s => s.Isactive == true && s.Guid == outputSheet.Seriesguid)
                .FirstOrDefault();

            outputSheet.SeriesTitle = assocSeries.Title;


            var sheet_item_guid = _context.CharacterSheetApproveds.Where(c => c.Isactive == true && c.Guid == guid)
                .FirstOrDefault().Fields.RootElement.GetProperty("Sheet_Item").GetString();


            if (sheet_item_guid != null)
            {
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                        .FirstOrDefault() != null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheetApproveds
                            .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                            .FirstOrDefault(),
                        _context);

                else if (_context.ItemSheets
                             .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                             .FirstOrDefault() !=
                         null)
                    outputSheet.Sheet_Item = Item.CreateItem(_context.ItemSheets
                            .Where(isa => isa.Guid.ToString() == sheet_item_guid && isa.Isactive == true)
                            .FirstOrDefault(),
                        _context);
            }

            var Start_Items = new List<IteSheet>();

            if (outputSheet.Sheet_Item != null && outputSheet.Sheet_Item.Islarge)
            {
                outputSheet.Sheet_Item.Issheetitem = true;
                Start_Items.Add(outputSheet.Sheet_Item);
            }

            var StartIguids = outputSheet.Fields["Starting_Items"].ToList();

            foreach (var iGuid in StartIguids)
                if (_context.ItemSheetApproveds
                        .Where(isa => isa.Isactive == true && isa.Guid.ToString() == iGuid.ToString())
                        .FirstOrDefault() != null)
                {
                    var starting_I = await _context.ItemSheetApproveds.Where(issh => issh.Isactive == true &&
                        issh.Guid.ToString() == iGuid.ToString()).FirstOrDefaultAsync();


                    Start_Items.Add(Item.CreateItem(starting_I, _context));
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

                    Start_Items.Add(Item.CreateItem(starting_I, _context));
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


    /// <summary>
    ///     Update an approved sheet WARNING WIZARD ONLY, this is to fix issues with approved sheets directly.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="characterSheetApproved"></param>
    /// <returns></returns>
    // PUT: api/CharacterSheetApproveds/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCharacterSheetApproved(int id, CharSheet characterSheetApproved)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
        //    if (characterSheetApproved.Img1 != null && characterSheetApproved.imagedata1 != null &&
        //        characterSheetApproved.imagedata1.Length != 0)
        //    {
        //        characterSheetApproved.Img1 = characterSheetApproved.Img1;

        //        var folderName = Path.Combine("images", "characters", "UnApproved");
        //        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        //        if (characterSheetApproved.imagedata1.Length > 0)
        //        {
        //            if (!Directory.Exists(pathToSave + "/"))
        //            {
        //                var di = Directory.CreateDirectory(pathToSave + "/");
        //            }

        //            System.IO.File.WriteAllBytes(pathToSave + "/" + characterSheetApproved.Img1,
        //                characterSheetApproved.imagedata1);
        //            ImageLogic.ResizeJpg(pathToSave + "/" + characterSheetApproved.Img1, true);
        //        }
        //    }

        //    if (characterSheetApproved.Img2 != null && characterSheetApproved.imagedata2 != null &&
        //        characterSheetApproved.imagedata2.Length != 0)
        //    {
        //        characterSheetApproved.Img2 = characterSheetApproved.Img2;

        //        var folderName = Path.Combine("images", "characters", "UnApproved");
        //        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        //        if (characterSheetApproved.imagedata2.Length > 0)
        //        {
        //            if (!Directory.Exists(pathToSave + "/"))
        //            {
        //                var di = Directory.CreateDirectory(pathToSave + "/");
        //            }

        //            System.IO.File.WriteAllBytes(pathToSave + "/" + characterSheetApproved.Img2,
        //                characterSheetApproved.imagedata2);
        //            ImageLogic.ResizeJpg(pathToSave + "/" + characterSheetApproved.Img2, false);
        //        }
        //    }

            _context.Entry(characterSheetApproved).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacterSheetApprovedExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Kick an approved sheet down to unapproved again. WIZARD ONLY
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [HttpPut("kick/{guid}")]
    public async Task<IActionResult> KickCharacterSheetApproved(Guid guid, string newReviewMessage)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var result = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();

            var approvedSheets = _context.CharacterSheetApproveds.Where(csa => csa.Isactive == true
                                                                               && csa.Guid == guid).ToList();

            if (approvedSheets.Count == 0) return NotFound();

            var selectedApprovedSheet = approvedSheets.Where(apps => apps.Id == approvedSheets.Max(aps => aps.Id))
                .FirstOrDefault();

            var selectedUnapprovedSheets = _context.CharacterSheets.Where(cs => cs.Guid == guid).ToList();

            var newUnaprovedsheet = new CharacterSheet
            {
                Guid = selectedApprovedSheet.Guid,
                Seriesguid = (Guid)selectedApprovedSheet.Seriesguid,
                Name = selectedApprovedSheet.Name,
                Img1 = selectedApprovedSheet.Img1,
                Img2 = selectedApprovedSheet.Img2,
                Fields = selectedApprovedSheet.Fields,
                Isactive = true,
                CreatedbyuserGuid = selectedApprovedSheet.CreatedbyuserGuid,
                Gmnotes = selectedApprovedSheet.Gmnotes,
                Version = selectedApprovedSheet.Version,
                EditbyUserGuid = result,
                Taglists = selectedApprovedSheet.Taglists
            };

            foreach (var sheet in approvedSheets) sheet.Isactive = false;

            foreach (var sheet in selectedUnapprovedSheets) sheet.Isactive = false;

            _context.CharacterSheets.Add(newUnaprovedsheet);

            await _context.SaveChangesAsync();

            var selectedUnapprovedSheet = _context.CharacterSheets.Where(cs => cs.Guid == guid
                                                                               && cs.Isactive == true && cs.Id ==
                                                                               _context.CharacterSheets
                                                                                   .Where(ccs => ccs.Isactive == true)
                                                                                   .Max(aps => aps.Id))
                .FirstOrDefault();


            var newMessage = new CharacterSheetReviewMessage
            {
                Isactive = true,
                CreatedbyuserGuid = result,
                Message = newReviewMessage,
                CharactersheetId = selectedUnapprovedSheet.Id
            };

            _context.CharacterSheetReviewMessages.Add(newMessage);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        return Unauthorized();
    }

    /// <summary>
    ///     POST new Approved sheet  WARNING: TO BE USED BY WIZARDS ONLY IN ORDER TO TRY AND REPLACE A DELETED ROW.
    /// </summary>
    /// <param name="characterSheetApproved"></param>
    /// <returns></returns>
    // POST: api/CharacterSheetApproveds
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    public async Task<ActionResult<CharacterSheetApproved>> PostCharacterSheetApproved(
        CharSheet characterSheetApproved)
    {

        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var characterSheet = new CharacterSheetApproved();

            characterSheet.CreatedbyuserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            //if (characterSheetApproved.Img1 != null && characterSheetApproved.imagedata1 != null &&
            //    characterSheetApproved.imagedata1.Length != 0)
            //{
            //    characterSheetApproved.Img1 = characterSheetApproved.Img1;

            //    var folderName = Path.Combine("images", "characters", "UnApproved");
            //    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            //    if (characterSheetApproved.imagedata1.Length > 0)
            //    {
            //        if (!Directory.Exists(pathToSave + "/"))
            //        {
            //            var di = Directory.CreateDirectory(pathToSave + "/");
            //        }

            //        System.IO.File.WriteAllBytes(pathToSave + "/" + characterSheetApproved.Img1,
            //            characterSheetApproved.imagedata1);
            //        ImageLogic.ResizeJpg(pathToSave + "/" + characterSheetApproved.Img1, true);
            //    }
            //}

            //if (characterSheetApproved.Img2 != null && characterSheetApproved.imagedata2 != null &&
            //    characterSheetApproved.imagedata2.Length != 0)
            //{
            //    characterSheetApproved.Img2 = characterSheetApproved.Img2;

            //    var folderName = Path.Combine("images", "characters", "UnApproved");
            //    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            //    if (characterSheetApproved.imagedata2.Length > 0)
            //    {
            //        if (!Directory.Exists(pathToSave + "/"))
            //        {
            //            var di = Directory.CreateDirectory(pathToSave + "/");
            //        }

            //        System.IO.File.WriteAllBytes(pathToSave + "/" + characterSheetApproved.Img2,
            //            characterSheetApproved.imagedata2);
            //        ImageLogic.ResizeJpg(pathToSave + "/" + characterSheetApproved.Img2, false);
            //    }
            //}

            _context.CharacterSheetApproveds.Add(characterSheet);
            await _context.SaveChangesAsync();


            return CreatedAtAction("PostCharacterSheetApproved", new { id = characterSheet.Id },
                characterSheetApproved);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Delete row from approved documents, to be used by Wizards ONLY.
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    // DELETE: api/CharacterSheetApproveds/5
    [HttpDelete("{guid}")]
    public async Task<ActionResult<CharacterSheetApproved>> DeleteCharacterSheetApproved(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var characterSheetApproved =
                await _context.CharacterSheetApproveds.Where(csa => csa.Guid == guid).ToListAsync();
            if (characterSheetApproved == null) return NotFound();

            foreach (var character in characterSheetApproved)
            {
                character.Isactive = false;
                _context.CharacterSheetApproveds.Update(character);
            }

            await _context.SaveChangesAsync();

            return characterSheetApproved.FirstOrDefault();
        }

        return Unauthorized();
    }

    private bool CharacterSheetApprovedExists(int id)
    {
        return _context.CharacterSheetApproveds.Any(e => e.Id == id);
    }
}