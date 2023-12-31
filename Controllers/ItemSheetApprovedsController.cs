using System;
using System.Collections.Generic;
using System.Linq;
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
public class ItemSheetApprovedsController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public ItemSheetApprovedsController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    // GET: api/v1/ItemSheetApproveds
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ItemSheetApproved>>> GetItemSheetApproved()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);


            return await _context.ItemSheetApproveds
                .Where(isa => isa.Isactive == true && allowedSheets.Contains(isa.Guid)).ToListAsync();
        }

        return Unauthorized();
    }

    // GET: api/v1/ItemSheetApproveds/5
    [HttpGet("{guid}")]
    [Authorize]
    public async Task<ActionResult<ItemSheetApproved>> GetItemSheetApproved(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var itemSheet = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid)
                .FirstOrDefaultAsync();

            if (itemSheet == null) return NotFound();

            var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
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
                if (System.IO.File.Exists(@"./images/items/Approved/" + outputItem.Img1))
                    outputItem.imagedata = System.IO.File.ReadAllBytes(@"./images/items/Approved/" + outputItem.Img1);

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

    // PUT: api/v1/ItemSheetApproveds/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutItemSheetApproved(int id, ItemSheetApproved itemSheetApproved)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            if (id != itemSheetApproved.Id) return BadRequest();

            _context.Entry(itemSheetApproved).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemSheetApprovedExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        return Unauthorized();
    }

    // GET: api/ItemSheetApproveds/5
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
                var itemsList = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid).ToListAsync();

                if (itemsList.Count == 0)
                {
                    return BadRequest("Item Not Found");
                }

                ListCharacterSheets output = new ListCharacterSheets();

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

                return output;
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        return Unauthorized();
    }


    [HttpGet("ShortListWithTags")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetApprovedItemListWithTags(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try
            {
                var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
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

                var outPutList = new List<IteSheet>();

                var allSheets = await _context.ItemSheetApproveds
                    .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                foreach (var sheet in allSheets)
                {
                    var newOutputSheet = new IteSheet
                    {
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
                    if (newOutputSheet.Img1 != null)
                        if (System.IO.File.Exists(@"./images/items/Approved/" + newOutputSheet.Img1))
                            newOutputSheet.imagedata =
                                System.IO.File.ReadAllBytes(@"./images/items/Approved/" + sheet.Img1);

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
                output.fulltotal = (allowedSheets.Count + pagingParameterModel.pageSize - 1) /
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
    public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetApprovedItemFullListWithTagsNoImages() {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try {
                var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
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

                var outPutList = new List<IteSheet>();

                var allSheets = await _context.ItemSheetApproveds
                    .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).OrderBy(x => x.Name).ToListAsync();

                foreach (var sheet in allSheets) {
                    var newOutputSheet = new IteSheet {
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
                output.fulltotal = allowedSheets.Count;

                return Ok(output);
            }
            catch (Exception e) {
                return BadRequest(e);
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
    public async Task<ActionResult<Series>> GetItemsSearchPartial(
        [FromQuery] ItemsPagingParameterModel pagingParameterModel)
    {
        if (pagingParameterModel.userApproved == true && pagingParameterModel.userCreated == true) return BadRequest();

        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.Fields)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt =>
                (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var initItems = await _context.ItemSheetApproveds
                .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

            if (pagingParameterModel.userCreated == true)
                initItems = initItems.Where(ii => ii.CreatedbyuserGuid == UsersLogic.GetUserGuid(authId, _context))
                    .ToList();
            if (pagingParameterModel.userCreated == false)
                initItems = initItems.Where(ii => ii.CreatedbyuserGuid != UsersLogic.GetUserGuid(authId, _context))
                    .ToList();

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
                                                  ii.SecondapprovalbyuserGuid != curUserGuid).ToList();
            }

            var taggedItems = new List<ItemSheetApproved>();
            var filteredItems = new List<ItemSheetApproved>();

            if (pagingParameterModel.fields != null && pagingParameterModel.fields != string.Empty)
            {
                var objects = JObject.Parse(pagingParameterModel.fields);

                foreach (var ite in initItems)
                {
                    var isfound = true;
                    foreach (var tag in objects)
                    {
                        var tagslist = new JsonElement();

                        if (ite.Fields.RootElement.TryGetProperty(tag.Key, out tagslist))
                        {
                            if (isfound && tag.Key == "Tags")
                            {
                                var TestJsonFeilds = ite.Fields.RootElement.GetProperty("Tags").EnumerateArray();
                                var tags2 = TestJsonFeilds.Select(s => Guid.Parse(s.GetString())).ToList();
                                var tags1 = tag.Value.ToObject<List<Guid>>();
                                var alltagsfound = tags1.Intersect(tags2).Count();
                                if (alltagsfound < tag.Value.ToArray().Length) isfound = false;
                            }

                            // when you hit special skills in the input JSON
                            else if (isfound && tag.Key == "Special_Skills")
                            {
                                var TestJsonFeilds = JArray.Parse(tagslist.ToString());

                                var skillsfound = new List<bool>();

                                // Iterate through the special skills array of the input json
                                foreach (JObject tagSkills in tag.Value)
                                    // iterate through all of the special skills on the item sheet
                                foreach (var itemSkills in TestJsonFeilds)
                                {
                                    var foundskills = new List<bool>();

                                    //iterate through all feilds of the input json
                                    foreach (var skillTag in tagSkills)
                                        if (skillTag.Key == "Tags")
                                        {
                                            var TagArray = JArray.Parse(itemSkills[skillTag.Key].ToString());
                                            var tags2 = TagArray.Select(s => Guid.Parse(s.ToString())).ToList();
                                            var tags1 = skillTag.Value.ToObject<List<Guid>>();
                                            var alltagsfound = tags1.Intersect(tags2).Count();
                                            if (alltagsfound == skillTag.Value.ToArray().Length) foundskills.Add(true);
                                        }
                                        else if (itemSkills[skillTag.Key].ToString().ToLower()
                                                 .Contains(skillTag.Value.ToString().ToLower()))
                                        {
                                            foundskills.Add(true);
                                        }

                                    if (foundskills.Count == tagSkills.Count) skillsfound.Add(true);
                                }

                                if (skillsfound.Count != tag.Value.ToList().Count) isfound = false;
                            }

                            else if (isfound)
                            {
                                if (!tagslist.ToString().ToLower().Contains(tag.Value.ToString().ToLower()))
                                    isfound = false;
                            }
                        }
                        else
                        {
                            isfound = false;
                        }
                    }

                    if (isfound) taggedItems.Add(ite);
                }
            }
            else
            {
                taggedItems = initItems;
            }

            foreach (var item in taggedItems)
            {
                var addItem = true;

                if (item.Fields.RootElement.TryGetProperty("Tags", out var TestJsonFeilds))
                {
                    var tagsList = TestJsonFeilds.EnumerateArray();
                    foreach (var tag in tagsList)
                        if (!allowedTags.Contains(Guid.Parse(tag.GetString())))
                            addItem = false;
                }

                if (addItem) filteredItems.Add(item);
            }

            var itemlistguids = filteredItems.Where(s =>
                pagingParameterModel.name == null ||
                s.Name.ToLower().Contains(pagingParameterModel.name.ToLower())).ToList();

            var itemslist = itemlistguids.OrderBy(x => x.Name)
                .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                .Take(pagingParameterModel.pageSize).ToList();

            if (itemslist == null) return NotFound();

            var outPutList = new List<IteSheet>();

            foreach (var sheet in itemslist)
            {
                var newOutputSheet = new IteSheet
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
                    Tags = new List<Tag>()
                };

                if (newOutputSheet.Img1 != null)
                    if (System.IO.File.Exists(@"./images/items/Approved/" + newOutputSheet.Img1))
                        newOutputSheet.imagedata =
                            System.IO.File.ReadAllBytes(@"./images/items/Approved/" + sheet.Img1);

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
            output.fulltotal = (itemlistguids.Count + pagingParameterModel.pageSize - 1) /
                               pagingParameterModel.pageSize;

            return Ok(output);
        }

        return Unauthorized();
    }


    // POST: api/v1/ItemSheetApproveds
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ItemSheetApproved>> PostItemSheetApproved(ItemSheetApproved itemSheetApproved)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            _context.ItemSheetApproveds.Add(itemSheetApproved);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItemSheetApproved", new { id = itemSheetApproved.Id }, itemSheetApproved);
        }

        return Unauthorized();
    }

    // DELETE: api/v1/ItemSheetApproveds/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ItemSheetApproved>> DeleteItemSheetApproved(Guid id)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var itemSheetApproved = await _context.ItemSheetApproveds.Where(i => i.Guid == id).ToListAsync();
            if (itemSheetApproved == null) return NotFound();

            foreach(var item in itemSheetApproved)
            {
                item.Isactive = false;
                _context.ItemSheetApproveds.Update(item);
            }
            _context.SaveChanges();

            return itemSheetApproved.FirstOrDefault();
        }

        return Unauthorized();
    }

    private bool ItemSheetApprovedExists(int id)
    {
        return _context.ItemSheetApproveds.Any(e => e.Id == id);
    }
}