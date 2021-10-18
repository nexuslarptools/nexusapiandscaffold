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
    public class TagsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public TagsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        // GET: api/v1/Tags
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Tags>>> GetTags()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                return await _context.Tags.ToListAsync();
            }
            return Unauthorized();
        }

        // GET: api/v1/Tags/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Tags>> GetTags(Guid id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var tags = await _context.Tags.FindAsync(id);

                if (tags == null)
                {
                    return NotFound();
                }

                return tags;
            }
            return Unauthorized();
        }

        // PUT: api/v1/Tags/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> PutTags(Guid guid, Tags tags)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                if (guid != tags.Guid)
                {
                    return BadRequest();
                }

                _context.Entry(tags).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagsExists(guid))
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

        // POST: api/v1/Tags
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Tags>> PostTags(Tags tags)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {
                _context.Tags.Add(tags);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTags", new { guid = tags.Guid }, tags);
            }
            return Unauthorized();
        }

        // DELETE: api/v1/Tags/5
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult<Tags>> DeleteTags(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var tags = await _context.Tags.FindAsync(guid);
                if (tags == null)
                {
                    return NotFound();
                }

                //tags.Isactive = false;

                _context.Tags.Remove(tags);
                await _context.SaveChangesAsync();

                return tags;
            }
            return Unauthorized();
        }

        private bool TagsExists(Guid id)
        {
            return _context.Tags.Any(e => e.Guid == id);
        }
    }
}
