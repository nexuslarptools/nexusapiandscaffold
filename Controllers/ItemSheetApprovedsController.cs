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
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ItemSheetApprovedsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public ItemSheetApprovedsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        // GET: api/v1/ItemSheetApproveds
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemSheetApproved>>> GetItemSheetApproved()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                return await _context.ItemSheetApproved.Where(isa => isa.Isactive==true).ToListAsync();
            }
            return Unauthorized();
        }

        // GET: api/v1/ItemSheetApproveds/5
        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<ItemSheetApproved>> GetItemSheetApproved(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var itemSheet = await _context.ItemSheetApproved.Where(ish => ish.Isactive == true && ish.Guid == guid).FirstOrDefaultAsync();

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
                    outputItem.imagedata = System.IO.File.ReadAllBytes(@"./images/items/Approved/"  + outputItem.Img1);
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

        // PUT: api/v1/ItemSheetApproveds/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutItemSheetApproved(int id, ItemSheetApproved itemSheetApproved)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                if (id != itemSheetApproved.Id)
                {
                    return BadRequest();
                }

                _context.Entry(itemSheetApproved).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemSheetApprovedExists(id))
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



        [HttpGet("ShortListWithTags")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<List<IteSheet>>>> GetApprovedItemListWithTags([FromQuery] PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                try
                {
                    List<IteSheet> outPutList = new List<IteSheet>();

                    var allSheets = await _context.ItemSheetApproved.Where(c => c.Isactive == true).OrderBy(x => x.Name)
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
                        if (newOutputSheet.Img1 != null)
                        {
                            newOutputSheet.imagedata = System.IO.File.ReadAllBytes(@"./images/items/Approved/" + sheet.Img1);
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



        // POST: api/v1/ItemSheetApproveds
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]

        public async Task<ActionResult<ItemSheetApproved>> PostItemSheetApproved(ItemSheetApproved itemSheetApproved)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {
                _context.ItemSheetApproved.Add(itemSheetApproved);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetItemSheetApproved", new { id = itemSheetApproved.Id }, itemSheetApproved);
            }
            return Unauthorized();
        }

        // DELETE: api/v1/ItemSheetApproveds/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ItemSheetApproved>> DeleteItemSheetApproved(int id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var itemSheetApproved = await _context.ItemSheetApproved.FindAsync(id);
                if (itemSheetApproved == null)
                {
                    return NotFound();
                }

                _context.ItemSheetApproved.Remove(itemSheetApproved);
                await _context.SaveChangesAsync();

                return itemSheetApproved;
            }

            return Unauthorized();
        }

        private bool ItemSheetApprovedExists(int id)
        {
            return _context.ItemSheetApproved.Any(e => e.Id == id);
        }
    }
}
