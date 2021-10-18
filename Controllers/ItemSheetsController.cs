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
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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



                return Ok(outputItem);


            }
            return Unauthorized();
        }



        [HttpGet("ShortListWithTags")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetItemListWithTags([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                List<IteSheet> outPutList = new List<IteSheet>();

                var allSheets = await _context.ItemSheet.Where(c => c.Isactive == true).OrderBy(x => x.Name)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                foreach (var sheet in allSheets)
                {
                    IteSheet newOutputSheet = new IteSheet
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
                        Tags = new List<Tags>(),
                    };

                    if (newOutputSheet.CreatedbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.CreatedbyuserGuid).FirstOrDefaultAsync();
                        newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.FirstapprovalbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.FirstapprovalbyuserGuid).FirstOrDefaultAsync();
                        newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                    }

                    if (newOutputSheet.SecondapprovalbyuserGuid != null)
                    {
                        var creUser = await _context.Users.Where(u => u.Guid == newOutputSheet.SecondapprovalbyuserGuid).FirstOrDefaultAsync();
                        newOutputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
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
            return Unauthorized();
        }




        [HttpGet("ByTag")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheet>>> GetItemSheetByTag([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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



        [HttpPut("{guid}/Approve")]
        [Authorize]
        public async Task<IActionResult> ApproveItemSheet(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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

                }




                return NoContent();
            }
            return Unauthorized();
        }




        // PUT: api/ItemSheets/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> PutItemSheet(Guid guid, [FromBody] IteSheet item)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBWrite"))
            {
                var itemSheet = await _context.ItemSheet.Where(s => s.Guid == guid).FirstOrDefaultAsync();

        
                if (guid != item.Guid || itemSheet == null)
                {
                    return BadRequest();
                }

                if (item.Name != null)
                {
                    itemSheet.Name = item.Name;
                }

                if (item.Img1 != null)
                {
                    itemSheet.Img1 = item.Img1;
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

                return NoContent();
            }
            return Unauthorized();
        }

        // POST: api/ItemSheets
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ItemSheet>> PostItemSheet([FromBody] IteSheet item)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBWrite"))
            {

                ItemSheet itemSheet = new ItemSheet();

                if (item.Name != null)
                {
                    itemSheet.Name = item.Name;
                }

                if (item.Img1 != null)
                {
                    itemSheet.Img1 = item.Img1;
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



                _context.ItemSheet.Add(itemSheet);
                await _context.SaveChangesAsync();

                return CreatedAtAction("PostItemSheet", new { id = itemSheet.Guid }, itemSheet);
            }
            return Unauthorized();
        }

        // DELETE: api/ItemSheets/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ItemSheet>> DeleteItemSheet(Guid id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var itemSheet = await _context.ItemSheet.FindAsync(id);
                if (itemSheet == null)
                {
                    return NotFound();
                }

                _context.ItemSheet.Remove(itemSheet);
                await _context.SaveChangesAsync();

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
