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
    public class PronounsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public PronounsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        // GET: api/v1/Pronouns
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Pronouns>>> GetPronouns()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                return await _context.Pronouns.ToListAsync();
            }
            return Unauthorized();
        }

        // GET: api/v1/Pronouns/5
        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Pronouns>> GetPronouns(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var pronouns = await _context.Pronouns.FindAsync(guid);

                if (pronouns == null)
                {
                    return NotFound();
                }

                return pronouns;
            }
            return Unauthorized();
        }

        // PUT: api/vi/Pronouns/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> PutPronouns(Guid guid, Pronouns pronouns)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                if (guid != pronouns.Guid)
                {
                    return BadRequest();
                }

                _context.Entry(pronouns).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PronounsExists(guid))
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

        // POST: api/Pronouns
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Pronouns>> PostPronouns(Pronouns pronouns)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                _context.Pronouns.Add(pronouns);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPronouns", new { id = pronouns.Guid }, pronouns);
            }
            return Unauthorized();
        }

        // DELETE: api/Pronouns/5
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult<Pronouns>> DeletePronouns(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var pronouns = await _context.Pronouns.FindAsync(guid);
                if (pronouns == null)
                {
                    return NotFound();
                }

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
}
