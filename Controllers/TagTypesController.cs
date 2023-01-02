using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    public class TagTypesController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public TagTypesController(NexusLARPContextBase context)
        {
            _context = context;
        }
        /// <summary>
        /// Returns all Tag Types and Guids
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/TagTypes
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TagTypes>>> GetTagTypes()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                return await _context.TagTypes.ToListAsync();
            }

            return Unauthorized();

        }




        /// <summary>
        /// Accepts the Guid of a TagsType selected and returns if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Tag type and list of all related tags</returns>
        // GET: api/v1/TagTypes/{guid}
        [HttpGet("AllTagsByType/{guid}")]
        [Authorize]
        public async Task<ActionResult<TagTypes>> GetTagsByType(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                var FindTagType = await _context.TagTypes.Where(tt => tt.Guid == guid).Select(t => new
                {
                    t.Guid,
                    t.Name,
                    tags =  _context.Tags.Where(tgs => tgs.Isactive==true && tgs.Tagtypeguid== guid).Select(ts => new 
                    { ts.Name, ts.Guid 
                    }).ToList() 

                }).FirstOrDefaultAsync();

                if (FindTagType == null)
                {
                    return NotFound();
                }


                return Ok(FindTagType);
            }
            return Unauthorized();
        }
        /// <summary>
        /// Accepts a Tag type's guid and JSON schema in the body to update information.  WIZARD ACCESS ONLY.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tagTypes"></param>
        /// <returns></returns>
        // PUT: api/V1/TagTypes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> PutTagTypes(Guid guid, [FromBody]TagTypes tagTypes)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                if (guid != tagTypes.Guid)
                {
                    return BadRequest();
                }

                _context.Entry(tagTypes).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagTypesExists(guid))
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
        /// Accepts tagtype schema JSON in the body and creates a new tag type. WIZARD ACCESS ONLY!
        /// </summary>
        /// <param name="tagTypes"></param>
        /// <returns></returns>
        // POST: api/TagTypes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TagTypes>> PostTagTypes(TagTypes tagTypes)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                _context.TagTypes.Add(tagTypes);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTagTypes", new { guid = tagTypes.Guid }, tagTypes);
            }
            return Unauthorized();
        }
        /// <summary>
        /// Deletes a TagType from the list WIZARD ACCESS ONLY, All related tags must be FULLY deleted first, may cause orhpaned guids in sheets!!
        /// </summary>
        /// <param name="tagTypes"></param>
        /// <returns></returns>
        // DELETE: api/TagTypes/5
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult<TagTypes>> DeleteTagTypes(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

            // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var tagTypes = await _context.TagTypes.FindAsync(guid);
                if (tagTypes == null)
                {
                    return NotFound();
                }

                _context.TagTypes.Remove(tagTypes);
                await _context.SaveChangesAsync();

                return tagTypes;
            }
            return Unauthorized();
        }

        private bool TagTypesExists(Guid id)
        {
            return _context.TagTypes.Any(e => e.Guid == id);
        }
    }
}
