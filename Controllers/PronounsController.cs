using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PronounsController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public PronounsController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    // GET: api/v1/Pronouns
    [HttpGet]
    [Authorize(Policy = "Reader")]
    public async Task<ActionResult<IEnumerable<PronounsOut>>> GetPronouns()
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (UsersLogic.IsUserAuthed(HttpContext.User, string.Empty, _context))
            return await _context.Pronouns.Select(p => new PronounsOut(p.Guid, p.Pronouns)).ToListAsync();
        return Unauthorized();
    }

    // GET: api/v1/Pronouns/5
    [HttpGet("{guid}")]
    [Authorize(Policy = "Reader")]
    public async Task<ActionResult<Pronoun>> GetPronouns(Guid guid)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Reader", _context))
        {
            var pronouns = await _context.Pronouns.FindAsync(guid);

            if (pronouns == null) return NotFound();

            return pronouns;
        }

        return Unauthorized();
    }

    // PUT: api/vi/Pronouns/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize(Policy = "Wizard")]
    public async Task<IActionResult> PutPronouns(Guid guid, Pronoun pronouns)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Wizard", _context))
        {
            if (guid != pronouns.Guid) return BadRequest();

            _context.Entry(pronouns).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PronounsExists(guid))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        return Unauthorized();
    }

    // POST: api/Pronouns
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize(Policy = "Wizard")]
    public async Task<ActionResult<Pronoun>> PostPronouns(Pronoun pronouns)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Wizard", _context))
        {
            _context.Pronouns.Add(pronouns);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPronouns", new { id = pronouns.Guid }, pronouns);
        }

        return Unauthorized();
    }

    // DELETE: api/Pronouns/5
    [HttpDelete("{guid}")]
    [Authorize(Policy = "Wizard")]
    public async Task<ActionResult<Pronoun>> DeletePronouns(Guid guid)
    {
        var authId = HttpContext.User.FindFirstValue("sub") ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UsersLogic.IsUserAuthed(HttpContext.User, "Wizard", _context))
        {
            var pronouns = await _context.Pronouns.FindAsync(guid);
            if (pronouns == null) return NotFound();

            _context.Pronouns.Remove(pronouns);
            await _context.SaveChangesAsync();

            return pronouns;
        }

        return Unauthorized();
    }

    private bool PronounsExists(Guid id)
    {
        return _context.Pronouns.Any(e => e.Guid == id);
    }
}
