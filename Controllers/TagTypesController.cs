using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TagTypesController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public TagTypesController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Returns all Tag Types and Guids
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/TagTypes
    [HttpGet]
    [Authorize(Policy = "Reader")]
    public async Task<ActionResult<IEnumerable<TagType>>> GetTagTypes()
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Reader", _context))
            return await _context.TagTypes.ToListAsync();

        return Unauthorized();
    }


    /// <summary>
    ///     Accepts the Guid of a TagsType selected and returns if it exists.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Tag type and list of all related tags</returns>
    // GET: api/v1/TagTypes/{guid}
    [HttpGet("AllTagsByType/{guid}")]
    [Authorize(Policy = "Reader")]
    public async Task<ActionResult<object>> GetTagsByType(Guid guid)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Reader", _context))
        {
            var FindTagType = await _context.TagTypes.Where(tt => tt.Guid == guid).Select(t => new
            {
                t.Guid,
                t.Name,
                tags = _context.Tags.Where(tgs => tgs.Isactive == true && tgs.Tagtypeguid == guid).Select(ts => new
                {
                    ts.Name, ts.Guid
                }).ToList()
            }).FirstOrDefaultAsync();

            if (FindTagType == null) return NotFound();


            return Ok(FindTagType);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Accepts a Tag type's guid and JSON schema in the body to update information.  WIZARD ACCESS ONLY.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tagTypes"></param>
    /// <returns></returns>
    // PUT: api/V1/TagTypes/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize(Policy = "Wizard")]
    public async Task<IActionResult> PutTagTypes(Guid guid, [FromBody] TagType tagTypes)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Wizard", _context))
        {
            if (guid != tagTypes.Guid) return BadRequest();

            _context.Entry(tagTypes).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagTypesExists(guid))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Accepts tagtype schema JSON in the body and creates a new tag type. WIZARD ACCESS ONLY!
    /// </summary>
    /// <param name="tagTypes"></param>
    /// <returns></returns>
    // POST: api/TagTypes
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize(Policy = "Wizard")]
    public async Task<ActionResult<TagType>> PostTagTypes(TagType tagTypes)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Wizard", _context))
        {
            _context.TagTypes.Add(tagTypes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTagTypes", new { guid = tagTypes.Guid }, tagTypes);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Deletes a TagType from the list WIZARD ACCESS ONLY, All related tags must be FULLY deleted first, may cause
    ///     orhpaned guids in sheets!!
    /// </summary>
    /// <param name="tagTypes"></param>
    /// <returns></returns>
    // DELETE: api/TagTypes/5
    [HttpDelete("{guid}")]
    [Authorize(Policy = "Wizard")]
    public async Task<ActionResult<TagType>> DeleteTagTypes(Guid guid)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Wizard", _context))
        {
            var tagTypes = await _context.TagTypes.FindAsync(guid);
            if (tagTypes == null) return NotFound();

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
