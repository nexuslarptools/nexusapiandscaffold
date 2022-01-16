using System;
using System.Collections.Generic;
using System.Linq;
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
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                return await _context.Tags.ToListAsync();
            }
            return Unauthorized();
        }

        // GET: api/v1/Tags
        [HttpGet("groupbytype")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TagsOutput>>> GetTagsGroupedByType()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                List<TagsOutput> output = new List<TagsOutput>();
                var tagTypes = await _context.TagTypes.ToListAsync();

                foreach (var type in tagTypes)
                {
                    TagsOutput outp = new TagsOutput();
                    outp.TagType = type.Name;

                    List<outTag> tagList = await _context.Tags.Where(t => t.Tagtypeguid == type.Guid && t.Isactive == true)
                        .Select(tt => new outTag(tt.Name, tt.Guid)).ToListAsync();

                    outp.TagsList = tagList.OrderBy(x => x.Name).ToList();

                    output.Add(outp);
                }

                return output;
            }
            return Unauthorized();
        }

        // GET: api/v1/Tags/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Tags>> GetTags(Guid id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
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
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
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

        /// <summary>
        /// Returns all Tag Types and Guids
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/Tags/
        [HttpGet("AllTagsByTypeName/{TypeName}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TagTypes>>> GetTagTypesWithTags(string TypeName)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var Foundtags = await _context.Tags.Where(t => t.Tagtypegu.Name == TypeName).ToListAsync();

                if (Foundtags == null)
                {
                    return NotFound();
                }

                return Ok(Foundtags.OrderBy(ft => ft.Name));
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
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
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
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                Tags tags = await _context.Tags.Where(t => t.Guid == guid).FirstOrDefaultAsync();
                if (tags == null)
                {
                    return NotFound();
                }

                //tags.Isactive = false;

                _context.Tags.Remove(tags);
                _context.SaveChanges();

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
