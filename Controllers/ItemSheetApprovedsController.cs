using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
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
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                var itemSheetApproved = await _context.ItemSheetApproved.Where(isa => isa.Isactive == true && isa.Guid == guid).FirstOrDefaultAsync();

                if (itemSheetApproved == null)
                {
                    return NotFound();
                }

                return itemSheetApproved;
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
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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

        // POST: api/v1/ItemSheetApproveds
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]

        public async Task<ActionResult<ItemSheetApproved>> PostItemSheetApproved(ItemSheetApproved itemSheetApproved)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
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
