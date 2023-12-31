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

[Route("api/v1/ItemSheets")]
[ApiController]
public class ItemSheetsController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public ItemSheetsController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    // GET: api/ItemSheets
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ItemSheet>>> GetItemSheet(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSehets = TagScanner.ScanTags(legalsheets, allowedTags);


            var sheets = await _context.ItemSheets.Where(s => s.Isactive == true && allowedSehets.Contains(s.Guid))
                .Select(sh => new
                {
                    sh.Guid,
                    sh.Name,
                    sh.Seriesguid,
                    sh.Series.Title,
                    sh.EditbyUserGuid,
                    Createdbyuser = sh.Createdbyuser.Preferredname,
                    sh.FirstapprovalbyuserGuid,
                    Firstapprovalbyuser = (sh.Firstapprovalbyuser.Preferredname == null || sh.Firstapprovalbyuser.Preferredname == string.Empty) ? sh.Firstapprovalbyuser.Firstname : sh.Firstapprovalbyuser.Preferredname,
                    sh.SecondapprovalbyuserGuid,
                    SecondApprovaluser = (sh.Secondapprovalbyuser.Preferredname == null || sh.Secondapprovalbyuser.Preferredname == string.Empty) ? sh.Secondapprovalbyuser.Firstname : sh.Secondapprovalbyuser.Preferredname,
                    EditbyUser = (sh.EditbyUser.Preferredname == null || sh.EditbyUser.Preferredname == string.Empty) ? sh.EditbyUser.Firstname : sh.EditbyUser.Preferredname,
                    CreatedBy = sh.Createdbyuser.Preferredname,
                })
                .OrderBy(x => x.Name)
                .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                .Take(pagingParameterModel.pageSize).ToListAsync();

            return Ok(sheets);
        }

        return Unauthorized();
    }

    // GET: api/ItemSheets/5
    [HttpGet("{guid}")]
    [Authorize]
    public async Task<ActionResult<ItemSheet>> GetItemSheet(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var itemSheet = await _context.ItemSheets.Where(ish => ish.Isactive == true && ish.Guid == guid)
                .FirstOrDefaultAsync();

            if (itemSheet == null) return NotFound();

            var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedShets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedShets.Contains(guid)) return Unauthorized();

            var outputItem = new IteSheet(itemSheet, _context);

            var tagslist = new JsonElement();

            itemSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

            if (tagslist.ValueKind.ToString() != "Undefined")
            {
                var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                foreach (var tag in TestJsonFeilds)
                {
                    var fullTag = await _context.Tags
                        .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                    outputItem.Tags.Add(fullTag);
                }
            }

            if (outputItem.Img1 != null)
                if (System.IO.File.Exists(@"./images/items/UnApproved/" + outputItem.Img1))
                    outputItem.imagedata = System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + outputItem.Img1);

            if (outputItem.Seriesguid != null)
            {
                var connectedSeries =
                    await _context.Series.Where(s => s.Guid == outputItem.Seriesguid).FirstOrDefaultAsync();
                if (connectedSeries != null) outputItem.Series = connectedSeries.Title;
            }


            return Ok(outputItem);
        }

        return Unauthorized();
    }

    // GET: api/ItemSheets/5
    [HttpGet("LinkedCharacters/{guid}")]
    [Authorize]
    public async Task<ActionResult<ListCharacterSheets>> GetAllCharactersLinkedItemSheet(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try
            {

                ListCharacterSheets output = new ListCharacterSheets();

                var itemsList = await _context.ItemSheets.Where(ish => ish.Isactive == true && ish.Guid == guid).ToListAsync();

                if (itemsList.Count == 0)
                {
                    return BadRequest("Item Not Found");
                }

                var itemsListApproved = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid).ToListAsync();

                if (itemsListApproved.Count > 0)
                {
                    return output;
                }

                var charList = _context.CharacterSheets.Where(cs => cs.Isactive == true).ToList();

                foreach (var characterSheet in charList)
                {
                    var startitemsList = new JsonElement();
                    characterSheet.Fields.RootElement.TryGetProperty("Starting_Items", out startitemsList);

                    if (startitemsList.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Starting_Items").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            if (tag.ToString() == guid.ToString())
                            {
                                output.AddUnapproved(characterSheet);
                            }
                        }

                        characterSheet.Fields.RootElement.TryGetProperty("Sheet_Item", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeild = characterSheet.Fields.RootElement.GetProperty("Sheet_Item");

                            if (TestJsonFeild.ToString() == guid.ToString())
                            {
                                output.AddUnapproved(characterSheet);
                            }
                        }
                    }
                }

                var approvcharList = _context.CharacterSheetApproveds.Where(cs => cs.Isactive == true).ToList();

                foreach (var characterSheet in approvcharList)
                {
                    var startitemsList = new JsonElement();
                    characterSheet.Fields.RootElement.TryGetProperty("Starting_Items", out startitemsList);

                    if (startitemsList.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Starting_Items").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            if (tag.ToString() == guid.ToString())
                            {
                                output.AddApproved(characterSheet);
                            }
                        }

                        characterSheet.Fields.RootElement.TryGetProperty("Sheet_Item", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeild = characterSheet.Fields.RootElement.GetProperty("Sheet_Item");

                            if (TestJsonFeild.ToString() == guid.ToString())
                            {
                                output.AddApproved(characterSheet);
                            }
                        }
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }


        return Unauthorized();
    }

    [HttpGet("MultiPick/")]
    [Authorize]
    public async Task<ActionResult<List<ItemSheet>>> GetItemSheetList([FromQuery] ItemSheetInput input)
    {
        //var input = JsonSerializer.Deserialize<ItemSheetInput>(guidjson);

        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var outputItemList = new List<IteSheet>();
            var outputItem = new IteSheet();


            var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles
                .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid)
                    .ToList();

            var allowedShets = TagScanner.ScanTags(legalsheets, allowedTags);

            foreach (var guid in input.A)
            {
                var outputAppedItem =
                    await GetApprovedItemSheetOutput(guid, allowedTags);

                outputItemList.Add(outputAppedItem);
            }

            foreach (var guid in input.U)
            {
                var itemSheet = await _context.ItemSheets.Where(ish => ish.Isactive == true && ish.Guid == guid)
                    .FirstOrDefaultAsync();

                if (itemSheet == null) return NotFound();

                if (!allowedShets.Contains(guid)) return Unauthorized();

                outputItem = Item.CreateItem(itemSheet);

                var tagslist = new JsonElement();

                itemSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        var fullTag = await _context.Tags
                            .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                            .FirstOrDefaultAsync();
                        outputItem.Tags.Add(fullTag);
                    }
                }

                if (outputItem.Img1 != null)
                    if (System.IO.File.Exists(@"./images/items/UnApproved/" + outputItem.Img1))
                        outputItem.imagedata =
                            System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + outputItem.Img1);

                if (outputItem.Seriesguid != null)
                {
                    var connectedSeries = await _context.Series.Where(s => s.Guid == outputItem.Seriesguid)
                        .FirstOrDefaultAsync();
                    if (connectedSeries != null) outputItem.Series = connectedSeries.Title;
                }

                outputItemList.Add(outputItem);
            }


            return Ok(outputItemList);
        }

        return Unauthorized();
    }


    [HttpGet("ShortListWithTags")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetItemListWithTags(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try
            {
                var outPutList = new List<IteSheet>();

                var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                    .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                var allowedLARPS = _context.UserLarproles
                    .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid)
                    .ToList();

                var allowedTags = _context.Larptags.Where(lt =>
                    (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                    && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                var allowedShets = TagScanner.ScanTags(legalsheets, allowedTags);

                var allSheets = await _context.ItemSheets.Where(c => c.Isactive == true && allowedShets.Contains(c.Guid))
                    .OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                foreach (var sheet in allSheets)
                {
                    var newOutputSheet = new IteSheet(sheet, _context);
                   /* {
                        Id = sheet.Id,
                        Guid = sheet.Guid,
                        Name = sheet.Name,
                        Img1 = sheet.Img1,
                        Seriesguid = sheet.Seriesguid,
                        Createdate = sheet.Createdate,
                        CreatedbyuserGuid = sheet.CreatedbyuserGuid,
                        FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid,
                        SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid,
                        Version = sheet.Version,
                        Tags = new List<Tag>()
                    };*/
                    if (newOutputSheet.Img1 != null)
                        if (System.IO.File.Exists(@"./images/items/UnApproved/" + newOutputSheet.Img1))
                            newOutputSheet.imagedata =
                                System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);

                    if (newOutputSheet.CreatedbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.CreatedbyuserGuid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.FirstapprovalbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.FirstapprovalbyuserGuid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.Firstapprovalby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.SecondapprovalbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.SecondapprovalbyuserGuid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.Secondapprovalby = creUser.Firstname + " " + creUser.Lastname;
                    }

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

                            newOutputSheet.Tags.Add(fullTag);
                        }
                    }

                    outPutList.Add(newOutputSheet);
                }

                var output = new IteListOut();
                output.IteList = outPutList.OrderBy(x => x.Name).ToList();
                output.fulltotal = (allowedShets.Count + pagingParameterModel.pageSize - 1) /
                                   pagingParameterModel.pageSize;

                return Ok(output);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        return Unauthorized();
    }

    [HttpGet("FullListWithTagsNoImages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetFullItemListWithTagsNoImages() 
        {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try {
                var outPutList = new List<IteSheet>();

                var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                    .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                var allowedLARPS = _context.UserLarproles
                    .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid)
                    .ToList();

                var allowedTags = _context.Larptags.Where(lt =>
                    (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                    && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                var allowedShets = TagScanner.ScanTags(legalsheets, allowedTags);

                var allSheets = await _context.ItemSheets.Where(c => c.Isactive == true && allowedShets.Contains(c.Guid))
                    .OrderBy(x => x.Name).ToListAsync();

                foreach (var sheet in allSheets) {
                    var newOutputSheet = new IteSheet {
                        Id = sheet.Id,
                        Guid = sheet.Guid,
                        Name = sheet.Name,
                        Img1 = sheet.Img1,
                        Seriesguid = sheet.Seriesguid,
                        Createdate = sheet.Createdate,
                        CreatedbyuserGuid = sheet.CreatedbyuserGuid,
                        FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid,
                        SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid,
                        Version = sheet.Version,
                        Tags = new List<Tag>()
                    };

                    if (newOutputSheet.CreatedbyuserGuid != null) {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.CreatedbyuserGuid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.FirstapprovalbyuserGuid != null) {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.FirstapprovalbyuserGuid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.Firstapprovalby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.SecondapprovalbyuserGuid != null) {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.SecondapprovalbyuserGuid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.Secondapprovalby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.Seriesguid != null) {
                        var ser = await _context.Series.Where(u => u.Guid == newOutputSheet.Seriesguid)
                            .FirstOrDefaultAsync();
                        newOutputSheet.Series = ser.Title;
                    }



                    var tagslist = new JsonElement();

                    sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined") {
                        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds) {
                            var fullTag = await _context.Tags
                                .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                                .FirstOrDefaultAsync();

                            newOutputSheet.Tags.Add(fullTag);
                        }
                    }

                    outPutList.Add(newOutputSheet);
                }

                var output = new IteListOut();
                output.IteList = outPutList.OrderBy(x => x.Name).ToList();
                output.fulltotal = allowedShets.Count;

                return Ok(output);
            }
            catch (Exception e) {
                return BadRequest(e);
            }

        return Unauthorized();
    }

    [HttpGet("FullListAllItems")]
    [Authorize]
    public async Task<ActionResult<FullItemsList>> FullListAllItems()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        var output = new FullItemsList();

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try
            {
                var outPutList = new List<IteSheet>();

                var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                    .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                var legalappprovedsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
                    .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                var allowedLARPS = _context.UserLarproles
                    .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid)
                    .ToList();

                var allowedTags = _context.Larptags.Where(lt =>
                    (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                    && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);
                var allowedApprovedSheets = TagScanner.ScanTags(legalappprovedsheets, allowedTags);

                output.ItemsList = await _context.ItemSheets
                    .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid))
                    .OrderBy(x => x.Name)
                    .Select(i => new ItemsEntry(i.Guid, i.Name)).ToListAsync();

                output.ApprovedItemsList = await _context.ItemSheetApproveds.Where(c =>
                        c.Isactive == true && allowedApprovedSheets.Contains(c.Guid))
                    .OrderBy(x => x.Name)
                    .Select(i => new ItemsEntry(i.Guid, i.Name)).ToListAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        return Ok(output);
    }


    [HttpGet("ByTag")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ItemSheet>>> GetItemSheetByTag(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();


            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (!allowedTags.Contains(pagingParameterModel
                    .guid))
                return Unauthorized();

            allowedTags = new List<Guid?>();

            allowedTags.Add(pagingParameterModel.guid);

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);


            //List<Guid> allFound = new List<Guid>();

            //var allSheets = await _context.ItemSheet.Where(c => c.Isactive == true).ToListAsync();

            //foreach (var sheet in allSheets)
            //{
            //    JsonElement tagslist = new JsonElement();

            //    sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);
            //    if (tagslist.ValueKind.ToString() != "Undefined")
            //    {
            //        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

            //        foreach (var tag in TestJsonFeilds)
            //        {
            //            if (tag.ToString() == pagingParameterModel.guid.ToString())
            //            {

            //                allFound.Add(sheet.Guid);
            //            }
            //        }
            //    }

            //}


            var output = await _context.ItemSheets.Where(c => c.Isactive == true &&
                                                             allowedSheets.Contains(c.Guid)).Select(ch => new
                {
                    ch.Name,
                    ch.Guid,
                    ch.Seriesguid,
                    SeriesTitle = _context.Series.Where(s => s.Isactive == true && s.Guid == ch.Seriesguid)
                        .FirstOrDefault().Title
                }).OrderBy(x => x.Name)
                .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                .Take(pagingParameterModel.pageSize).ToListAsync();


            return Ok(output);
        }

        return Unauthorized();
    }


    /// <summary>
    ///     Search for series based on partial series name.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Items/Search
    [HttpGet("Search/")]
    [Authorize]
    public async Task<ActionResult<ItemSheet>> GetItemsSearchPartial(
        [FromQuery] ItemsPagingParameterModel pagingParameterModel)
    {
        if (pagingParameterModel.userApproved == true && pagingParameterModel.userCreated == true) return BadRequest();

        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                try
                {

                    List<TagScanContainer> legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                        .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                    List<Guid> allowedLARPS = _context.UserLarproles
                        .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                        .Select(ulr => (Guid)ulr.Larpguid).ToList();

                    var allowedTags = _context.Larptags.Where(lt =>
                        (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                        && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                    if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                    {
                        allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid)
                            .ToList();
                    }

                    var initItems = await _context.ItemSheets.Where(c => c.Isactive == true).ToListAsync();

                    if (pagingParameterModel.userCreated == true)
                    {
                        initItems = initItems
                            .Where(ii => ii.CreatedbyuserGuid == UsersLogic.GetUserGuid(authId, _context)).ToList();
                    }

                    if (pagingParameterModel.userCreated == false)
                    {
                        initItems = initItems
                            .Where(ii => ii.CreatedbyuserGuid != UsersLogic.GetUserGuid(authId, _context)).ToList();
                    }

                    if (pagingParameterModel.userApproved == true)
                    {
                        var curUserGuid = UsersLogic.GetUserGuid(authId, _context);
                        initItems = initItems.Where(ii => ii.FirstapprovalbyuserGuid == curUserGuid ||
                                                          ii.SecondapprovalbyuserGuid == curUserGuid).ToList();
                    }

                    if (pagingParameterModel.userApproved == false)
                    {
                        var curUserGuid = UsersLogic.GetUserGuid(authId, _context);
                        initItems = initItems.Where(ii => ii.FirstapprovalbyuserGuid != curUserGuid &&
                                                          ii.SecondapprovalbyuserGuid != curUserGuid).OrderBy(x => x.Name).ToList();
                    }

                    var taggedItems = new List<ItemSheet>();

                    if (!string.IsNullOrEmpty(pagingParameterModel.fields))
                    {
                        var objects = JObject.Parse(pagingParameterModel.fields);

                        foreach (var ite in initItems)
                        {
                            bool isfound = true;
                            foreach (var tag in objects)
                            {
                                JsonElement tagslist = new JsonElement();

                                if (ite.Fields.RootElement.TryGetProperty(tag.Key, out tagslist))
                                {
                                    if (isfound && tag.Key == "Tags")
                                    {
                                        var TestJsonFeilds =
                                            ite.Fields.RootElement.GetProperty("Tags").EnumerateArray();
                                        var tags2 = TestJsonFeilds.Select(s => Guid.Parse(s.GetString())).ToList();
                                        List<Guid> tags1 = tag.Value.ToObject<List<Guid>>();
                                        var alltagsfound = tags1.Intersect(tags2).Count();

                                        foreach (var tagValue in tags2)
                                        {
                                            if (!allowedTags.Contains(tagValue))
                                            {
                                                isfound = false;
                                            }
                                        }

                                        if (alltagsfound < tag.Value.ToArray().Length)
                                        {
                                            isfound = false;
                                        }
                                    }

                                    // when you hit special skills in the input JSON
                                    else if (isfound && tag.Key == "Special_Skills")
                                    {
                                        var TestJsonFeilds = JArray.Parse(tagslist.ToString());

                                        List<bool> skillsfound = new List<bool>();

                                        // Iterate through the special skills array of the input json
                                        foreach (JObject tagSkills in tag.Value)
                                        {
                                            // iterate through all of the special skills on the item sheet
                                            foreach (var itemSkills in TestJsonFeilds)
                                            {
                                                List<bool> foundskills = new List<bool>();

                                                //iterate through all feilds of the input json
                                                foreach (var skillTag in tagSkills)
                                                {
                                                    if (skillTag.Key == "Tags")
                                                    {
                                                        var TagArray =
                                                            JArray.Parse(itemSkills[skillTag.Key].ToString());
                                                        var tags2 = TagArray.Select(s => Guid.Parse(s.ToString()))
                                                            .ToList();
                                                        List<Guid> tags1 = skillTag.Value.ToObject<List<Guid>>();
                                                        var alltagsfound = tags1.Intersect(tags2).Count();
                                                        if (alltagsfound == skillTag.Value.ToArray().Length)
                                                        {
                                                            foundskills.Add(true);
                                                        }

                                                    }
                                                    else if (itemSkills[skillTag.Key].ToString().ToLower()
                                                             .Contains(skillTag.Value.ToString().ToLower()))
                                                    {
                                                        foundskills.Add(true);
                                                    }
                                                }

                                                if (foundskills.Count == tagSkills.Count)
                                                {
                                                    skillsfound.Add(true);
                                                }

                                            }

                                        }

                                        if (skillsfound.Count != tag.Value.ToList().Count)
                                        {
                                            isfound = false;
                                        }


                                    }

                                    else if (isfound)
                                    {
                                        if (!tagslist.ToString().ToLower().Contains(tag.Value.ToString().ToLower()))
                                        {
                                            isfound = false;
                                        }

                                    }
                                }
                                else
                                {
                                    isfound = false;
                                }


                            }

                            if (isfound)
                            {
                                taggedItems.Add(ite);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in initItems)
                        {
                            var addItem = true;

                            if (item.Fields.RootElement.TryGetProperty("Tags", out var TestJsonFeilds))
                            {
                                var tagsList = TestJsonFeilds.EnumerateArray();
                                foreach (var tag in tagsList)
                                {
                                    if (!allowedTags.Contains(Guid.Parse(tag.GetString())))
                                    {
                                        addItem = false;
                                    }
                                }
                            }

                            if (addItem)
                            {
                                taggedItems.Add(item);
                            }
                        }

                    }

                    var filteredGuids = taggedItems.Where(s =>
                            (pagingParameterModel.name == null ||
                             s.Name.ToLower().Contains(pagingParameterModel.name.ToLower()))
                            && (pagingParameterModel.seriesguid == Guid.Empty ||
                                s.Seriesguid == pagingParameterModel.seriesguid))
                        .OrderBy(x => x.Name).Select(ti => ti.Guid).ToList();


                    var itemslist = taggedItems.Where(s =>
                            (pagingParameterModel.name == null ||
                             s.Name.ToLower().Contains(pagingParameterModel.name.ToLower()))
                            && (pagingParameterModel.seriesguid == Guid.Empty ||
                                s.Seriesguid == pagingParameterModel.seriesguid))
                        .OrderBy(x => x.Name)
                        .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                        .Take(pagingParameterModel.pageSize).ToList();

                    if (itemslist == null)
                    {
                        return NotFound();
                    }

                    List<IteSheet> outPutList = new List<IteSheet>();

                    foreach (var sheet in itemslist)
                    {

                        IteSheet newOutputSheet = new IteSheet
                        {
                            Id = sheet.Id,
                            Guid = sheet.Guid,
                            Name = sheet.Name,
                            Img1 = sheet.Img1,
                            Seriesguid = sheet.Seriesguid,
                            Createdate = sheet.Createdate,
                            CreatedbyuserGuid = sheet.CreatedbyuserGuid,
                            FirstapprovalbyuserGuid = sheet.FirstapprovalbyuserGuid,
                            SecondapprovalbyuserGuid = sheet.SecondapprovalbyuserGuid,
                            Version = sheet.Version,
                            Tags = new List<Tag>(),
                        };
                        if (newOutputSheet.Img1 != null)
                        {
                            if (System.IO.File.Exists(@"./images/items/UnApproved/" + sheet.Img1))
                            {
                                newOutputSheet.imagedata =
                                    System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);
                            }
                        }

                        if (newOutputSheet.CreatedbyuserGuid != null)
                        {
                            var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.CreatedbyuserGuid)
                                .FirstOrDefaultAsync();
                            newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                        }

                        if (newOutputSheet.FirstapprovalbyuserGuid != null)
                        {
                            var creUser = await _context.Users
                                .Where(u => u.Guid == newOutputSheet.FirstapprovalbyuserGuid)
                                .FirstOrDefaultAsync();
                            newOutputSheet.Firstapprovalby = creUser.Firstname + " " + creUser.Lastname;
                        }

                        if (newOutputSheet.SecondapprovalbyuserGuid != null)
                        {
                            var creUser = await _context.Users
                                .Where(u => u.Guid == newOutputSheet.SecondapprovalbyuserGuid)
                                .FirstOrDefaultAsync();
                            newOutputSheet.Secondapprovalby = creUser.Firstname + " " + creUser.Lastname;
                        }

                        JsonElement tagslist = new JsonElement();

                        sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                        if (tagslist.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                            foreach (var tag in TestJsonFeilds)
                            {
                                var fullTag = await _context.Tags
                                    .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString()))
                                    .FirstOrDefaultAsync();
                                newOutputSheet.Tags.Add(fullTag);

                            }
                        }


                        outPutList.Add(newOutputSheet);


                    }

                    var output = new IteListOut();
                    output.IteList = outPutList.OrderBy(x => x.Name).ToList();
                    output.fulltotal = (filteredGuids.Count + pagingParameterModel.pageSize - 1) /
                                       pagingParameterModel.pageSize;

                    return Ok(output);
                }
                catch (Exception ex)
                {
                    return Problem(ex.Message);
                }
            }
            return Unauthorized();
        }


    [HttpPut("{guid}/Approve")]
    [Authorize]
    public async Task<IActionResult> ApproveItemSheet(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Approver", _context))
        {
            var approvaltype = "first";
            var result = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            var itemSheet = await _context.ItemSheets.Where(cs => cs.Isactive == true && cs.Guid == guid)
                .FirstOrDefaultAsync();

            var fullapprove = false;

            if (itemSheet == null) return BadRequest();

            var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedShets = TagScanner.ScanTags(legalsheets, allowedTags);

            if (!allowedShets.Contains(guid)) return Unauthorized();

            if (itemSheet.FirstapprovalbyuserGuid != null && itemSheet.SecondapprovalbyuserGuid == null)
            {
                if (result != itemSheet.CreatedbyuserGuid && result != itemSheet.FirstapprovalbyuserGuid)
                {
                    itemSheet.SecondapprovalbyuserGuid = result;
                    itemSheet.Secondapprovaldate = DateTime.Now;
                    itemSheet.Isactive = false;
                    fullapprove = true;
                }
                else
                {
                    return Unauthorized();
                }
            }


            if (itemSheet.FirstapprovalbyuserGuid == null)
            {
                if (result != itemSheet.CreatedbyuserGuid)
                {
                    itemSheet.FirstapprovalbyuserGuid = result;
                    itemSheet.Firstapprovaldate = DateTime.Now;
                }
                else
                {
                    return Unauthorized();
                }
            }

            _context.Update(itemSheet);


            int? maxversion = 0;

            if (fullapprove)
            {
                approvaltype = "second";
                var approvedSheets = await _context.ItemSheetApproveds
                    .Where(csa => csa.Guid == guid).ToListAsync();

                if (approvedSheets.Count > 0)
                {
                    maxversion = approvedSheets.MaxBy(ash => ash.Version).Version;
                }

                maxversion++;

                foreach (var asheet in approvedSheets)
                {
                    asheet.Isactive = false;
                    _context.ItemSheetApproveds.Update(asheet);
                }

                var newapproval = new ItemSheetApproved
                {
                    Guid = itemSheet.Guid,
                    ItemsheetId = itemSheet.Id,
                    Seriesguid = itemSheet.Seriesguid,
                    Name = itemSheet.Name,
                    Img1 = itemSheet.Img1,
                    Fields = itemSheet.Fields,
                    Isactive = true,
                    Createdate = DateTime.Now,
                    CreatedbyuserGuid = itemSheet.CreatedbyuserGuid,
                    FirstapprovalbyuserGuid = itemSheet.FirstapprovalbyuserGuid,
                    Firstapprovaldate = itemSheet.Firstapprovaldate,
                    SecondapprovalbyuserGuid = itemSheet.SecondapprovalbyuserGuid,
                    Secondapprovaldate = itemSheet.Secondapprovaldate,
                    Gmnotes = itemSheet.Gmnotes,
                    Version = maxversion
                };

                _context.ItemSheetApproveds.Add(newapproval);

                var CurrfolderName = Path.Combine("images", "items", "UnApproved");
                var NewfolderName = Path.Combine("images", "items", "Approved");
                var pathfrom = Path.Combine(Directory.GetCurrentDirectory(), CurrfolderName,
                    itemSheet.Img1);
                var pathto = Path.Combine(Directory.GetCurrentDirectory(), NewfolderName, itemSheet.Img1);

                if (System.IO.File.Exists(pathto)) System.IO.File.Delete(pathto);

                System.IO.File.Move(pathfrom, pathto);
            }

            await _context.SaveChangesAsync();

            //var theNewSheet = await _context.ItemSheetApproved
            //    .Where(csa => csa.Guid == guid && csa.Version > maxversion).FirstOrDefaultAsync();

            //if (theNewSheet != null && fullapprove)
           // {
           //     theNewSheet.Isactive = true;
           //     theNewSheet.SecondapprovalbyuserGuid = result;
           //     theNewSheet.Secondapprovaldate = DateTime.Now;

           //     _context.ItemSheetApproved.Update(theNewSheet);
           //     await _context.SaveChangesAsync();
           // }

            return Ok("{\"Approval\":\"" + approvaltype + "\"}");
        }

        return Unauthorized();
    }


    // PUT: api/ItemSheets/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize]
    public async Task<ActionResult<IteSheet>> PutItemSheet(Guid guid, [FromBody] IteSheetInput item)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            try
            {

                if (guid != item.Guid) return BadRequest();


                string pathToSave = string.Empty;
                var itemSheetList = await _context.ItemSheets.Where(s => s.Guid == guid)
                .OrderBy(s => s.Version).ToListAsync();

                ItemSheet itemSheet = item.OutputToItemSheet();
                itemSheet.CreatedbyuserGuid =
    _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
                itemSheet.EditbyUserGuid =
    _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();

                if (itemSheetList.Count > 0)
                {
                    foreach (var isheet in itemSheetList)
                    {
                        isheet.Isactive = false;
                        _context.Update(isheet);
                        _context.SaveChanges();
                    }

                    itemSheet.Version = itemSheetList.MaxBy(csa => csa.Version).Version + 1;
                    itemSheet.CreatedbyuserGuid = itemSheetList.MaxBy(csa => csa.Version).CreatedbyuserGuid;
                }

                List<TagScanContainer> legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                    .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
                List<Guid> allowedLARPS = _context.UserLarproles
                    .Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                    .Select(ulr => (Guid)ulr.Larpguid).ToList();

                var allowedTags = _context.Larptags.Where(lt =>
                    (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                    && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

                if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid)
                        .ToList();
                }

                List<Guid> allowedShets = TagScanner.ScanTags(legalsheets, allowedTags);

                if (item.Img1 != null && item.imagedata != null && item.imagedata.Length != 0)
                {
                    itemSheet.Img1 = item.Img1;

                    var folderName = Path.Combine("images", "items", "UnApproved");
                    pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    if (item.imagedata.Length > 0 && pathToSave != string.Empty)
                    {
                        if (!Directory.Exists(pathToSave + "/"))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(pathToSave + "/");
                        }

                        System.IO.File.WriteAllBytes(pathToSave + "/" + item.Img1, item.imagedata);
                    }

                }

                if (item.Fields != null)
                {

                    foreach (var tag in item.Fields)
                    {

                        if (tag.Key == "Tags")
                        {
                            var TestJsonFeilds = item.Fields["Tags"];

                            foreach (Guid tagValue in TestJsonFeilds)
                            {
                                if (!allowedTags.Contains(tagValue))
                                {
                                    return Unauthorized();
                                }
                            }
                        }

                    }
                }

                _context.Add(itemSheet);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemSheetExists(guid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var nwoutputItem = Extensions.Item.CreateItem(itemSheet);

                return nwoutputItem;
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        return Unauthorized();
    }

    // POST: api/ItemSheets
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [DisableRequestSizeLimit]
    [Authorize]
    public async Task<ActionResult<IteSheet>> PostItemSheet([FromBody] IteSheet item)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var legalsheets = _context.ItemSheets.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var itemSheet = new ItemSheet();

            if (item.Guid == null) item.Guid = Guid.NewGuid();

            itemSheet.Guid = item.Guid;
            if (item.Name != null) itemSheet.Name = item.Name;

            if (item.Img1 != null && item.imagedata != null && item.imagedata.Length != 0)
            {
                itemSheet.Img1 = item.Img1;

                var folderName = Path.Combine("images", "items", "UnApproved");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (item.imagedata.Length > 0)
                {
                    if (!Directory.Exists(pathToSave + "/"))
                    {
                        var di = Directory.CreateDirectory(pathToSave + "/");
                    }

                    System.IO.File.WriteAllBytes(pathToSave + "/" + item.Img1, item.imagedata);
                }
            }

            if (item.Gmnotes != null) itemSheet.Gmnotes = item.Gmnotes;

            if (item.Fields != null)
            {
                foreach (var tag in item.Fields)
                    if (tag.Key == "Tags")
                    {
                        var TestJsonFeilds = item.Fields["Tags"];

                        foreach (Guid tagValue in TestJsonFeilds)
                            if (!allowedTags.Contains(tagValue))
                                return Unauthorized();
                    }

                itemSheet.Fields = JsonDocument.Parse(item.Fields.ToString());
            }

            if (item.Reason4edit != null) itemSheet.Reason4edit = item.Reason4edit;

            if (item.Seriesguid != null) itemSheet.Seriesguid = item.Seriesguid;

            itemSheet.Createdate = DateTime.Now;
            itemSheet.CreatedbyuserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            itemSheet.EditbyUserGuid =
                _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            itemSheet.FirstapprovalbyuserGuid = null;
            itemSheet.Firstapprovaldate = null;
            itemSheet.SecondapprovalbyuserGuid = null;
            itemSheet.Secondapprovaldate = null;
            itemSheet.Isactive = true;


            var isheets = await _context.ItemSheets.Where(cs => cs.Guid == itemSheet.Guid).ToListAsync();
            if (isheets != null && isheets.Count > 0)
            {
                var maxsheet = isheets.MaxBy(csa => csa.Version);
                itemSheet.CreatedbyuserGuid = maxsheet.CreatedbyuserGuid;
                itemSheet.Version = maxsheet.Version;
                itemSheet.Version++;
            }

            try
            {
                _context.ItemSheets.Add(itemSheet);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var nwoutputItem = Item.CreateItem(itemSheet);

            return Ok(nwoutputItem);
        }

        return Unauthorized();
    }

    // DELETE: api/ItemSheets/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ItemSheet>> DeleteItemSheet(Guid id)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var itemSheets = await _context.ItemSheets.Where(i => i.Guid == id).ToListAsync();
            if (itemSheets == null) return NotFound();

            try
            {
                foreach (var item in itemSheets)
                {
                    item.Isactive = false;
                    _context.ItemSheets.Update(item);
                    _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return itemSheets.FirstOrDefault();
        }

        return Unauthorized();
    }

    private async Task<IteSheet> GetApprovedItemSheetOutput(Guid guid, List<Guid?> allowedTags)
    {
        var itemSheet = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid)
            .FirstOrDefaultAsync();

        var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
            .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();

        if (itemSheet == null) return null;

        var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

        if (!allowedSheets.Contains(guid)) return null;

        var outputItem = Item.CreateItem(itemSheet);

        var tagslist = new JsonElement();

        itemSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

        if (tagslist.ValueKind.ToString() != "Undefined")
        {
            var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

            foreach (var tag in TestJsonFeilds)
            {
                var fullTag = await _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                outputItem.Tags.Add(fullTag);
            }
        }

        if (outputItem.Img1 != null)
            if (System.IO.File.Exists(@"./images/items/Approved/" + outputItem.Img1))
                outputItem.imagedata = System.IO.File.ReadAllBytes(@"./images/items/Approved/" + outputItem.Img1);

        if (outputItem.Seriesguid != null)
        {
            var connectedSeries =
                await _context.Series.Where(s => s.Guid == outputItem.Seriesguid).FirstOrDefaultAsync();
            if (connectedSeries != null) outputItem.Series = connectedSeries.Title;
        }

        return outputItem;
    }

    private bool ItemSheetExists(Guid id)
    {
        return _context.ItemSheets.Any(e => e.Guid == id);
    }
}