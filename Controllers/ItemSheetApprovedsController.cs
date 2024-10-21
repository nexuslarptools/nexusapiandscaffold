using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public async Task<ActionResult<IEnumerable<ItemSheetApproved>>> GetItemSheetApproved()
    {
        var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
            .Select(it => new TagScanContainer(it.Guid, it.ItemSheetApprovedTags)).ToList();
        var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Isactive == true)
            .Select(ulr => (Guid)ulr.Larpguid).ToList();

        var allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

        return await _context.ItemSheetApproveds
            .Where(isa => isa.Isactive == true && allowedSheets.Contains(isa.Guid)).ToListAsync();
    }

    // GET: api/v1/ItemSheetApproveds/5
    [HttpGet("{guid}")]
    public async Task<ActionResult<ItemSheetApproved>> GetItemSheetApproved(Guid guid)
    {
        var itemSheet = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid)
            .FirstOrDefaultAsync();

        if (itemSheet == null) return NotFound();

        var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
            .Select(it => new TagScanContainer(it.Guid, it.ItemSheetApprovedTags)).ToList();
        var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Isactive == true)
            .Select(ulr => (Guid)ulr.Larpguid).ToList();

        var allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

        if (!allowedSheets.Contains(guid)) return Unauthorized();

        var outputItem = new IteSheet(itemSheet, _context);

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

    // GET: api/v1/ItemSheetApproveds/5
    [HttpGet("getlastavailable/{guid}")]
    public async Task<ActionResult<ItemSheetApproved>> GetItemSheetLastAvailable(Guid guid)
    {
        var itemSheet = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid)
            .FirstOrDefaultAsync();

        if (itemSheet == null)
        {

            var unappitemSheet = await _context.ItemSheets.Where(ish => ish.Isactive == true && ish.Guid == guid)
        .FirstOrDefaultAsync();
            var legalunappsheets = _context.ItemSheets.Where(it => it.Isactive == true)
        .Select(it => new TagScanContainer(it.Guid, it.ItemSheetTags)).ToList();
            var allowedunappLARPS = _context.UserLarproles.Where(ulr => ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedunappTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedunappSheets = TagScanner.ScanTags(legalunappsheets, allowedunappTags);

            if (!allowedunappSheets.Contains(guid)) return Unauthorized();

            var outputunappItem = new IteSheet(unappitemSheet, _context);

            if (outputunappItem.Img1 != null)
                if (System.IO.File.Exists(@"./images/items/Approved/" + outputunappItem.Img1))
                    outputunappItem.imagedata = System.IO.File.ReadAllBytes(@"./images/items/Approved/"
                  + outputunappItem.Img1);

            if (outputunappItem.Seriesguid != null)
            {
                var connectedSeries =
                    await _context.Series.Where(s => s.Guid == outputunappItem.Seriesguid).FirstOrDefaultAsync();
                if (connectedSeries != null) outputunappItem.Series = connectedSeries.Title;
            }

            return Ok(outputunappItem);

        }

        var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
            .Select(it => new TagScanContainer(it.Guid, it.ItemSheetApprovedTags)).ToList();
        var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Isactive == true)
            .Select(ulr => (Guid)ulr.Larpguid).ToList();

        var allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

        if (!allowedSheets.Contains(guid)) return Unauthorized();

        var outputItem = new IteSheet(itemSheet, _context);

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

    // PUT: api/v1/ItemSheetApproveds/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutItemSheetApproved(int id, ItemSheetApproved itemSheetApproved)
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

    // GET: api/ItemSheetApproveds/5
    [HttpGet("LinkedCharacters/{guid}")]
    public async Task<ActionResult<ListItemsAndCharacters>> GetAllCharactersLinkedItemSheet(Guid guid)
    {
        try
        {
            var itemsList = await _context.ItemSheetApproveds.Where(ish => ish.Isactive == true && ish.Guid == guid).ToListAsync();

            if (itemsList.Count == 0)
            {
                return BadRequest("Item Not Found");
            }

            ListCharacterSheets output = new ListCharacterSheets();
            ListItemsAndCharacters fullOutput = new ListItemsAndCharacters();

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

            fullOutput.CharacterLists = output;

            return fullOutput;
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }


    [HttpGet("ShortListWithTags")]
    public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetApprovedItemListWithTags(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        try
        {
            var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.ItemSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles
                .Where(ulr => ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid)
                .ToList();

            var allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

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
                    Tags = new List<TagOut>()
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
                            .Include("Tagtype").FirstOrDefaultAsync();
                        newOutputSheet.Tags.Add(new TagOut(fullTag));
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
    }

    [HttpGet("FullListWithTagsNoImages")]
    public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetFullItemListWithTagsNoImagesRedo()
    {
        try
        {
            var wizardauth = true;
            var outPutList = new List<IteSheet>();

            var allowedLARPS = _context.UserLarproles
               .Where(ulr => ulr.Isactive == true).Select(ulr => (Guid)ulr.Larpguid)
               .ToList();

            var disAllowedTags = _context.Larptags.Where(lt =>
                !(allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
                && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allSheets = await _context.ItemSheetApproveds.Where(c => c.Isactive == true)
.Select(x => new ItemSheetApprovedDO
{
    Sheet = new ItemSheetApproved
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
    },
    TagList = x.ItemSheetApprovedTags.Select(ist => ist.Tag).ToList(),
    Createdbyuser = x.Createdbyuser,
    EditbyUser = x.EditbyUser,
    Firstapprovalbyuser = x.Firstapprovalbyuser,
    Secondapprovalbyuser = x.Secondapprovalbyuser,
    Series = x.Series,
    ListMessages = _context.ItemSheetReviewMessages.Where(isrm => isrm.Isactive == true
      && isrm.ItemsheetId == x.Id).ToList()
}).ToListAsync();

            if (!wizardauth)
            {
                allSheets = allSheets.Where(ash => !disAllowedTags.Any(dat => ash.TagList.Any(tl => tl.Guid == dat))).ToList();


                //  await _context.ItemSheetApproveds.Where(c => c.Isactive == true
                //    && (!JsonConvert.DeserializeObject<TagsObject>(c.Taglists)
                //  .MainTags.Any(mt => disAllowedTags.Contains(mt)) &&
                //  !JsonConvert.DeserializeObject<TagsObject>(c.Taglists)
                //  .AbilityTags.Any(mt => disAllowedTags.Contains(mt))))
                //   .OrderBy(x => x.Name).ToListAsync();
            }

            foreach (var sheet in allSheets)
            {

                var newOutputSheet = new IteSheet(sheet, _context);
                outPutList.Add(newOutputSheet);

            }

            var output = new IteListOut();
            output.IteList = outPutList.OrderBy(x => x.Name).ToList();
            output.fulltotal = outPutList.Count;

            return Ok(output);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    ///     Search for series based on partial series name.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Items/Search
    [HttpGet("Search/")]
    public async Task<ActionResult<Series>> GetItemsSearchPartial(
        [FromQuery] ItemsPagingParameterModel pagingParameterModel)
    {
        if (pagingParameterModel.userApproved == true && pagingParameterModel.userCreated == true) return BadRequest();

        {
            var legalsheets = _context.ItemSheetApproveds.Where(it => it.Isactive == true)
                .Select(it => new TagScanContainer(it.Guid, it.ItemSheetApprovedTags)).ToList();
            var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.Isactive == true)
                .Select(ulr => (Guid)ulr.Larpguid).ToList();

            var allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

            var allowedSheets = TagScanner.ScanTags(legalsheets, allowedTags);

            var initItems = await _context.ItemSheetApproveds
                .Where(c => c.Isactive == true && allowedSheets.Contains(c.Guid)).ToListAsync();

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
                    Tags = new List<TagOut>()
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
                            .Include("Tagtype").FirstOrDefaultAsync();
                        newOutputSheet.Tags.Add(new TagOut(fullTag));
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
    }


    /// <summary>
    /// Kick an approved sheet down to unapproved again. WIZARD ONLY
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    [HttpPut("kick/{guid}")]
    public async Task<IActionResult> KickItemSheetApproved(Guid guid, string newReviewMessage)
    {
        var result = _context.Users.Select(u => u.Guid).FirstOrDefault();

        var approvedSheets = _context.ItemSheetApproveds.Where(csa => csa.Isactive == true
            && csa.Guid == guid).ToList();

        if (approvedSheets.Count == 0)
        {
            return NotFound();
        }

        var selectedApprovedSheet = approvedSheets.Where(apps => apps.Id == approvedSheets.Max(aps => aps.Id)).FirstOrDefault();

        var selectedUnapprovedSheets = _context.ItemSheets.Where(cs => cs.Guid == guid).ToList();

        ItemSheet newUnaprovedsheet = new ItemSheet()
        {
            Guid = selectedApprovedSheet.Guid,
            Seriesguid = (Guid)selectedApprovedSheet.Seriesguid,
            Name = selectedApprovedSheet.Name,
            Img1 = selectedApprovedSheet.Img1,
            Fields = selectedApprovedSheet.Fields,
            Isactive = true,
            CreatedbyuserGuid = selectedApprovedSheet.CreatedbyuserGuid,
            Gmnotes = selectedApprovedSheet.Gmnotes,
            Version = selectedApprovedSheet.Version,
            EditbyUserGuid = result,
            Taglists = selectedApprovedSheet.Taglists
        };

        foreach (var sheet in approvedSheets)
        {
            sheet.Isactive = false;
        }

        foreach (var sheet in selectedUnapprovedSheets)
        {
            sheet.Isactive = false;
        }

        _context.ItemSheets.Add(newUnaprovedsheet);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception Ex)
        {
            throw;
        }

        var selectedUnapprovedSheet = _context.ItemSheets.Where(cs => cs.Guid == guid
          && cs.Isactive == true && cs.Id == _context.ItemSheets.Where(ccs => ccs.Isactive == true)
          .Max(aps => aps.Id)).FirstOrDefault();


        ItemSheetReviewMessage newMessage = new ItemSheetReviewMessage()
        {
            Isactive = true,
            CreatedbyuserGuid = result,
            Message = newReviewMessage,
            ItemsheetId = selectedUnapprovedSheet.Id
        };


        _context.ItemSheetReviewMessages.Add(newMessage);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception Ex)
        {
            throw;
        }

        return NoContent();
    }

    // POST: api/v1/ItemSheetApproveds
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ItemSheetApproved>> PostItemSheetApproved(ItemSheetApproved itemSheetApproved)
    {
        _context.ItemSheetApproveds.Add(itemSheetApproved);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetItemSheetApproved", new { id = itemSheetApproved.Id }, itemSheetApproved);
    }

    // DELETE: api/v1/ItemSheetApproveds/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<ItemSheetApproved>> DeleteItemSheetApproved(Guid id)
    {
        var itemSheetApproved = await _context.ItemSheetApproveds.Where(i => i.Guid == id).ToListAsync();
        if (itemSheetApproved == null) return NotFound();

        foreach (var item in itemSheetApproved)
        {
            item.Isactive = false;
            _context.ItemSheetApproveds.Update(item);
        }
        _context.SaveChanges();

        return itemSheetApproved.FirstOrDefault();
    }
    private bool ItemSheetApprovedExists(int id)
    {
        return _context.ItemSheetApproveds.Any(e => e.Id == id);
    }
}