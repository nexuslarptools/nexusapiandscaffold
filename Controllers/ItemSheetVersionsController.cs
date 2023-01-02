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
    public class ItemSheetVersionsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public ItemSheetVersionsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        // GET: api/ItemSheetVersions
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemSheetVersion>>> GetItemSheetVersion()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Approver", _context))
            {
                var itemlist = await _context.ItemSheetVersion.Select(isv => new { 
                isv.Id,
                isv.Guid,
                isv.Name,
                isv.Seriesguid,
                isv.Seriesgu.Title,
                isv.Version}).ToListAsync();

                return Ok(itemlist);
            }
            return Unauthorized();
        }

        /// <summary>
        /// Gets the list of all versions of one Item Sheet.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        // GET: api/ItemSheetVersions
        [HttpGet("AllVersionsofItem/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ItemSheetVersion>>> GetItemSheetVersionsByItemGuid(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Approver", _context))
            {
                var itemlist = await _context.ItemSheetVersion.Where(iv => iv.Guid==guid).Select(isv => new {
                    isv.Id,
                    isv.Guid,
                    isv.Name,
                    isv.Seriesguid,
                    isv.Seriesgu.Title,
                    isv.Version
                }).ToListAsync();

                return Ok(itemlist);
            }
            return Unauthorized();
        }

        // GET: api/ItemSheetVersions/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ItemSheetVersion>> GetItemSheetVersion(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Approver", _context))
            {
                var itemSheetVersion = await _context.ItemSheetVersion.FindAsync(id);

                var outputSheet = Item.CreateItem(itemSheetVersion);


                if (outputSheet.CreatedbyuserGuid != null)
                {
                    var creUser = await _context.Users.Where(u => u.Guid == outputSheet.CreatedbyuserGuid).FirstOrDefaultAsync();
                    outputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                }

                if (outputSheet.FirstapprovalbyuserGuid != null)
                {
                    var creUser = await _context.Users.Where(u => u.Guid == outputSheet.FirstapprovalbyuserGuid).FirstOrDefaultAsync();
                    outputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                }

                if (outputSheet.SecondapprovalbyuserGuid != null)
                {
                    var creUser = await _context.Users.Where(u => u.Guid == outputSheet.SecondapprovalbyuserGuid).FirstOrDefaultAsync();
                    outputSheet.createdby = creUser.Firstname + " " + creUser.Lastname;
                }


                if (itemSheetVersion == null)
                {
                    return NotFound();
                }

                return Ok(outputSheet);
            }
            return Unauthorized();
        }

        // PUT: api/ItemSheetVersions/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutItemSheetVersion(int id, ItemSheetVersion itemSheetVersion)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                if (id != itemSheetVersion.Id)
                {
                    return BadRequest();
                }

                _context.Entry(itemSheetVersion).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemSheetVersionExists(id))
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
        /// Updates a entry on the version table.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="characterSheetVersion"></param>
        /// <returns></returns>
        // PUT: api/CharacterSheetVersions/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("revert/{id}")]
        [Authorize]
        public async Task<IActionResult> RevertItemSheetVersion(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                var oldSheet = await _context.ItemSheetVersion.Where(csv => csv.Id == id).FirstOrDefaultAsync();

                if (oldSheet == null)
                {
                    return BadRequest();
                }


                var editSheet = await _context.ItemSheet.Where(cs => cs.Guid == oldSheet.Guid).FirstOrDefaultAsync();

                editSheet.Seriesguid = oldSheet.Seriesguid;
                editSheet.Name = oldSheet.Name;
                editSheet.Img1 = oldSheet.Img1;
                editSheet.Fields = JsonDocument.Parse(oldSheet.Fields.RootElement.ToString());
                editSheet.Isactive = true;
                editSheet.Createdate = DateTime.Now;
                editSheet.CreatedbyuserGuid = _context.Users.Where(u => u.Authid==authId).Select(u => u.Guid).FirstOrDefault();
                editSheet.FirstapprovalbyuserGuid = null;
                editSheet.Firstapprovaldate = null;
                editSheet.SecondapprovalbyuserGuid = null;
                editSheet.Secondapprovaldate = null;
                editSheet.Gmnotes = null;
                editSheet.Reason4edit = null;

                _context.ItemSheet.Update(editSheet);
                await _context.SaveChangesAsync();

                return Ok(editSheet);

            }
            return Unauthorized();
        }

        // POST: api/ItemSheetVersions
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ItemSheetVersion>> PostItemSheetVersion(ItemSheetVersion itemSheetVersion)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                _context.ItemSheetVersion.Add(itemSheetVersion);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetItemSheetVersion", new { id = itemSheetVersion.Id }, itemSheetVersion);
            }
            return Unauthorized();
        }

        // DELETE: api/ItemSheetVersions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ItemSheetVersion>> DeleteItemSheetVersion(int id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {
                var itemSheetVersion = await _context.ItemSheetVersion.FindAsync(id);
                if (itemSheetVersion == null)
                {
                    return NotFound();
                }

                _context.ItemSheetVersion.Remove(itemSheetVersion);
                await _context.SaveChangesAsync();

                return itemSheetVersion;
            }
            return Unauthorized();
        }

        private bool ItemSheetVersionExists(int id)
        {
            return _context.ItemSheetVersion.Any(e => e.Id == id);
        }
    }
}
