using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Extensions;
using System.IO;
using System.Net.Http.Headers;
using NEXUSDataLayerScaffold.Logic;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/ItemSheets")]
    [ApiController]
    public class ItemSheetsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public ItemSheetsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        // GET: api/ItemSheets
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemSheet>>> GetItemSheet([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var sheets = await _context.ItemSheet.Where(s => s.Isactive==true ).Select(sh => new { 
                sh.Guid,
                sh.Name,
                sh.Seriesguid,
                sh.Seriesgu.Title
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
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var itemSheet = await _context.ItemSheet.Where(ish => ish.Isactive == true && ish.Guid == guid).FirstOrDefaultAsync();

                if (itemSheet == null)
                {
                    return NotFound();
                }


                var outputItem = Extensions.Item.CreateItem(itemSheet);

                JsonElement tagslist = new JsonElement();

                itemSheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                if (tagslist.ValueKind.ToString() != "Undefined")
                {
                    var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                    foreach (var tag in TestJsonFeilds)
                    {
                        Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                        outputItem.Tags.Add(fullTag);
                    }
                }

                if (outputItem.Img1 != null)
                {
                    if (System.IO.File.Exists(@"./images/items/UnApproved/" + outputItem.Img1))
                    {
                        outputItem.imagedata = System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + outputItem.Img1);
                    }
                }

                if (outputItem.Seriesguid != null)
                {
                    var connectedSeries = await _context.Series.Where(s => s.Guid == outputItem.Seriesguid).FirstOrDefaultAsync();
                    if (connectedSeries != null)
                    {
                        outputItem.Series = connectedSeries.Title;
                    }

}


                return Ok(outputItem);


            }
            return Unauthorized();
        }



        [HttpGet("ShortListWithTags")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetItemListWithTags([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                try
                {
                    List<IteSheet> outPutList = new List<IteSheet>();

                    var allSheets = await _context.ItemSheet.Where(c => c.Isactive == true).OrderBy(x => x.Name)
                        .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                        .Take(pagingParameterModel.pageSize).ToListAsync();

                    foreach (var sheet in allSheets)
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
                            Tags = new List<Tags>(),
                        };
                        if (newOutputSheet.Img1 != null)
                        {
                            if (System.IO.File.Exists(@"./images/items/UnApproved/" + newOutputSheet.Img1))
                            {
                                newOutputSheet.imagedata = System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);
                            }
                        }

                        if (newOutputSheet.CreatedbyuserGuid != null)
                        {
                            var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.CreatedbyuserGuid).FirstOrDefaultAsync();
                            newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                        }

                        if (newOutputSheet.FirstapprovalbyuserGuid != null)
                        {
                            var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.FirstapprovalbyuserGuid).FirstOrDefaultAsync();
                            newOutputSheet.Firstapprovalby = creUser.Firstname + " " + creUser.Lastname;
                        }

                        if (newOutputSheet.SecondapprovalbyuserGuid != null)
                        {
                            var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.SecondapprovalbyuserGuid).FirstOrDefaultAsync();
                            newOutputSheet.Secondapprovalby = creUser.Firstname + " " + creUser.Lastname;
                        }

                        JsonElement tagslist = new JsonElement();

                        sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                        if (tagslist.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                            foreach (var tag in TestJsonFeilds)
                            {
                                Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                                newOutputSheet.Tags.Add(fullTag);
                            }
                        }

                        outPutList.Add(newOutputSheet);

                    }


                    return Ok(outPutList.OrderBy(x => x.Name));
                }
                catch (Exception e)
                {
                    return BadRequest(e);
                }
            }
            return Unauthorized();
        }




        [HttpGet("ByTag")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemSheet>>> GetItemSheetByTag([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                List<Guid> allFound = new List<Guid>();

                var allSheets = await _context.ItemSheet.Where(c => c.Isactive == true).ToListAsync();

                foreach (var sheet in allSheets)
                {
                    JsonElement tagslist = new JsonElement();

                    sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);
                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            if (tag.ToString() == pagingParameterModel.guid.ToString())
                            {

                                allFound.Add(sheet.Guid);
                            }
                        }
                    }


                }


                var output = await _context.ItemSheet.Where(c => c.Isactive == true &&
               allFound.Contains(c.Guid)).Select(ch => new
               {
                   ch.Name,
                   ch.Guid,
                   ch.Seriesguid,
                   SeriesTitle = _context.Series.Where(s => s.Isactive == true && s.Guid == ch.Seriesguid).FirstOrDefault().Title
               }).OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();


                return Ok(output);
            }
            return Unauthorized();
        }


        /// <summary>
        /// Search for series based on partial series name.
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/Items/Search
        [HttpGet("Search/")]
        [Authorize]
        public async Task<ActionResult<ItemSheet>> GetItemsSearchPartial([FromQuery] ItemsPagingParameterModel pagingParameterModel)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                var initItems = await _context.ItemSheet.Where(c => c.Isactive == true).ToListAsync();

                var taggedItems = new List<ItemSheet>();

                if (pagingParameterModel.fields != null && pagingParameterModel.fields != string.Empty)
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
                                    var TestJsonFeilds = ite.Fields.RootElement.GetProperty("Tags").EnumerateArray();
                                    var tags2 = TestJsonFeilds.Select(s => Guid.Parse(s.GetString())).ToList();
                                    List<Guid> tags1 = tag.Value.ToObject<List<Guid>>();
                                    var alltagsfound = tags1.Intersect(tags2).Count();
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
                                                    var TagArray = JArray.Parse(itemSkills[skillTag.Key].ToString());
                                                    var tags2 = TagArray.Select(s => Guid.Parse(s.ToString())).ToList();
                                                    List<Guid> tags1 = skillTag.Value.ToObject<List<Guid>>();
                                                    var alltagsfound = tags1.Intersect(tags2).Count();
                                                    if (alltagsfound == skillTag.Value.ToArray().Length)
                                                    {
                                                        foundskills.Add(true);
                                                    }

                                                }
                                                else if (itemSkills[skillTag.Key].ToString().ToLower().Contains(skillTag.Value.ToString().ToLower()))
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
                    taggedItems = initItems;
                }

                var itemslist = taggedItems.Where(s => 
                (pagingParameterModel.name == null || s.Name.ToLower().Contains(pagingParameterModel.name.ToLower()))
                && (pagingParameterModel.seriesguid == Guid.Empty || s.Seriesguid == pagingParameterModel.seriesguid))
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
                        Tags = new List<Tags>(),
                    };
                    if (newOutputSheet.Img1 != null)
                    {
                        if (System.IO.File.Exists(@"./images/items/UnApproved/" + sheet.Img1))
                        { 
                            newOutputSheet.imagedata = System.IO.File.ReadAllBytes(@"./images/items/UnApproved/" + sheet.Img1);
                        }
                    }

                    if (newOutputSheet.CreatedbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.CreatedbyuserGuid).FirstOrDefaultAsync();
                        newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.FirstapprovalbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.FirstapprovalbyuserGuid).FirstOrDefaultAsync();
                        newOutputSheet.Firstapprovalby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.SecondapprovalbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.SecondapprovalbyuserGuid).FirstOrDefaultAsync();
                        newOutputSheet.Secondapprovalby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    JsonElement tagslist = new JsonElement();

                    sheet.Fields.RootElement.TryGetProperty("Tags", out tagslist);

                    if (tagslist.ValueKind.ToString() != "Undefined")
                    {
                        var TestJsonFeilds = sheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                        foreach (var tag in TestJsonFeilds)
                        {
                            Tags fullTag = await _context.Tags.Where(t => t.Isactive == true && t.Guid == Guid.Parse(tag.GetString())).FirstOrDefaultAsync();
                            newOutputSheet.Tags.Add(fullTag);
                        }
                    }

                    outPutList.Add(newOutputSheet);

                }


                return Ok(outPutList);
            }
            return Unauthorized();
        }



        [HttpPut("{guid}/Approve")]
        [Authorize]
        public async Task<IActionResult> ApproveItemSheet(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBApprover"))
            {

                ItemSheet itemSheet = await _context.ItemSheet.Where(cs => cs.Isactive == true && cs.Guid == guid).FirstOrDefaultAsync();
                bool fullapprove = false;

                if (itemSheet == null)
                {
                    return BadRequest();
                }

                if (itemSheet.FirstapprovalbyuserGuid != null && itemSheet.SecondapprovalbyuserGuid == null)
                {
                    if (result.Result.userGuid != itemSheet.CreatedbyuserGuid && result.Result.userGuid != itemSheet.FirstapprovalbyuserGuid)
                    {
                        itemSheet.SecondapprovalbyuserGuid = result.Result.userGuid;
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
                    if (result.Result.userGuid != itemSheet.CreatedbyuserGuid)
                    {
                        //characterSheet.SecondapprovalbyuserGuid = null;
                        itemSheet.FirstapprovalbyuserGuid = result.Result.userGuid;
                        itemSheet.Firstapprovaldate = DateTime.Now;
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }


                _context.Entry(itemSheet).State = EntityState.Modified;

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

                if (fullapprove)
                {
                    var approvedSheets = await _context.ItemSheetApproved.Where(csa => csa.Guid == guid).ToListAsync();

                    int maxversion = 0;

                    foreach (var asheet in approvedSheets)
                    {
                        if (maxversion < asheet.Version)
                        {
                            maxversion = asheet.Version;
                        }

                        asheet.Isactive = false;

                        _context.ItemSheetApproved.Update(asheet);
                        await _context.SaveChangesAsync();

                    }

                    var theNewSheet = await _context.ItemSheetApproved.Where(csa => csa.Guid == guid && csa.Version == maxversion).FirstOrDefaultAsync();

                    theNewSheet.Isactive = true;
                    theNewSheet.SecondapprovalbyuserGuid = result.Result.userGuid;
                    theNewSheet.Secondapprovaldate = DateTime.Now;

                    _context.ItemSheetApproved.Update(theNewSheet);
                    _context.SaveChanges();


                    if (theNewSheet.Img1 != null )
                    {
                        var CurrfolderName = Path.Combine("images", "items", "UnApproved");
                        var NewfolderName = Path.Combine("images", "items", "Approved");
                        var pathfrom = Path.Combine(Directory.GetCurrentDirectory(), CurrfolderName, theNewSheet.Img1);
                        var pathto = Path.Combine(Directory.GetCurrentDirectory(), NewfolderName, theNewSheet.Img1);

                        if (System.IO.File.Exists(pathto))
                        {
                            System.IO.File.Delete(pathto);
                        }
                        System.IO.File.Move(pathfrom, pathto);

                        return Ok("{\"Approval\":\"Second\"}");

                    }

                }
                return Ok("{\"Approval\":\"First\"}");
            }
            return Unauthorized();
        }




        // PUT: api/ItemSheets/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult<ItemSheet>> PutItemSheet(Guid guid, [FromBody] IteSheetInput item)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBWrite"))
            {
                string pathToSave = string.Empty;
                var itemSheet = await _context.ItemSheet.Where(s => s.Guid == guid && s.Isactive == true).FirstOrDefaultAsync();

        
                if (guid != item.Guid || itemSheet == null)
                {
                   var itemSheetApp = await _context.ItemSheetApproved.Where(s => s.Guid == guid && s.Isactive == true).FirstOrDefaultAsync();

                    if (guid != item.Guid || itemSheetApp == null)
                    {
                        return BadRequest();
                    }

                    ItemSheet newitemSheet = new ItemSheet();

                    if (item.Guid != null)
                    {
                        newitemSheet.Guid = item.Guid;
                    }

                    if (item.Name != null)
                    {
                        newitemSheet.Name = item.Name;
                    }

                    if (item.Gmnotes != null)
                    {
                        newitemSheet.Gmnotes = item.Gmnotes;
                    }

                    if (item.Fields != null)
                    {
                        newitemSheet.Fields = JsonDocument.Parse(item.Fields.ToString());
                    }

                    if (item.Reason4edit != null)
                    {
                        newitemSheet.Reason4edit = item.Reason4edit;
                    }

                    if (item.Seriesguid != null)
                    {
                        newitemSheet.Seriesguid = item.Seriesguid;
                    }

                    newitemSheet.Createdate = DateTime.Now;
                    newitemSheet.CreatedbyuserGuid = result.Result.userGuid;
                    newitemSheet.FirstapprovalbyuserGuid = null;
                    newitemSheet.Firstapprovaldate = null;
                    newitemSheet.SecondapprovalbyuserGuid = null;
                    newitemSheet.Secondapprovaldate = null;
                    newitemSheet.Isactive = true;

                    if (item.Img1 != null && item.imagedata != null && item.imagedata.Length != 0)
                    {
                        newitemSheet.Img1 = item.Img1;

                        var folderName = Path.Combine("images", "items", "UnApproved");
                        pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    }

                    _context.ItemSheet.Add(newitemSheet);
                    await _context.SaveChangesAsync();

                    if (item.imagedata.Length > 0 && pathToSave != string.Empty)
                    {
                        if (!Directory.Exists(pathToSave + "/"))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(pathToSave + "/");
                        }
                        System.IO.File.WriteAllBytes(pathToSave + "/" + item.Img1, item.imagedata);
                    }



                    return newitemSheet;

                }

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

                if (item.Img1 != null)
                {
                    itemSheet.Img1 = item.Img1;
                }


                if (item.Name != null)
                {
                    itemSheet.Name = item.Name;
                }

                if (item.Gmnotes != null)
                {
                    itemSheet.Gmnotes = item.Gmnotes;
                }

                if (item.Fields != null)
                {
                    itemSheet.Fields = JsonDocument.Parse(item.Fields.ToString());
                }

                if (item.Reason4edit != null)
                {
                    itemSheet.Reason4edit = item.Reason4edit;
                }

                if (item.Seriesguid != null)
                {
                    itemSheet.Seriesguid = item.Seriesguid;
                }


                itemSheet.Createdate = DateTime.Now;
                itemSheet.CreatedbyuserGuid = result.Result.userGuid;
                itemSheet.FirstapprovalbyuserGuid = null;
                itemSheet.Firstapprovaldate = null;
                itemSheet.SecondapprovalbyuserGuid = null;
                itemSheet.Secondapprovaldate = null;
                itemSheet.Isactive = true;


                _context.Entry(itemSheet).State = EntityState.Modified;

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

                return itemSheet;
            }
            return Unauthorized();
        }

        // POST: api/ItemSheets
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost, DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ItemSheet>> PostItemSheet([FromBody] IteSheet item)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBWrite"))
            {

                ItemSheet itemSheet = new ItemSheet();

                if (item.Guid == null)
                {
                    item.Guid = Guid.NewGuid();
                }

                itemSheet.Guid = item.Guid;
                if (item.Name != null)
                {
                    itemSheet.Name = item.Name;
                }

                if (item.Img1 != null &&  item.imagedata != null && item.imagedata.Length != 0)
                {
                    itemSheet.Img1 = item.Img1;

                    var folderName = Path.Combine("images", "items", "UnApproved");
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                    if (item.imagedata.Length > 0)
                    {
                        if (!Directory.Exists(pathToSave + "/"))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(pathToSave + "/");
                        }
                        System.IO.File.WriteAllBytes(pathToSave + "/" + item.Img1, item.imagedata);
                    }

                }

                if (item.Gmnotes != null)
                {
                    itemSheet.Gmnotes = item.Gmnotes;
                }

                if (item.Fields != null)
                {
                    itemSheet.Fields = JsonDocument.Parse(item.Fields.ToString());
                }

                if (item.Reason4edit != null)
                {
                    itemSheet.Reason4edit = item.Reason4edit;
                }

                if (item.Seriesguid != null)
                {
                    itemSheet.Seriesguid = item.Seriesguid;
                }

                itemSheet.Createdate = DateTime.Now;
                itemSheet.CreatedbyuserGuid = result.Result.userGuid;
                itemSheet.FirstapprovalbyuserGuid = null;
                itemSheet.Firstapprovaldate = null;
                itemSheet.SecondapprovalbyuserGuid = null;
                itemSheet.Secondapprovaldate = null;
                itemSheet.Isactive = true;


                try
                {
                    _context.ItemSheet.Add(itemSheet);
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }

                return CreatedAtAction("GetItem", new { id = itemSheet.Guid }, itemSheet);
            }
            return Unauthorized();
        }

        // DELETE: api/ItemSheets/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ItemSheet>> DeleteItemSheet(Guid id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var itemSheet = await _context.ItemSheet.Where(i => i.Guid == id).FirstOrDefaultAsync();
                if (itemSheet == null)
                {
                    return NotFound();
                }

                _context.ItemSheet.Remove(itemSheet);
                _context.SaveChanges();

                return itemSheet;
            }
            return Unauthorized();
        }

        private bool ItemSheetExists(Guid id)
        {
            return _context.ItemSheet.Any(e => e.Guid == id);
        }
    }
}
