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
    public class LarpsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public LarpsController(NexusLARPContextBase context)
        {
            _context = context;
        }

        // GET: api/Larps
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Larps>>> GetLarps()
        {
            return await _context.Larps.ToListAsync();
        }

        // GET: api/Larps/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Larps>> GetLarps(Guid id)
        {
            var larps = await _context.Larps.FindAsync(id);

            if (larps == null)
            {
                return NotFound();
            }

            return larps;
        }

        // PUT: api/Larps/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<Larps>> PutLarps(Guid id, Larps larps)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

                if (!result.Result.roles.Contains("Wizard") && !result.Result.roles.Contains(larps.Shortname + " Head GM"))
            {
                return Unauthorized();
            }

            var currentLarp = _context.Larps.Where(l => l.Isactive==true && l.Guid==id).FirstOrDefault();

            if (currentLarp == null)
            {
                return BadRequest();
            }



            Larps newLarp = new Larps
            {
                Name = larps.Name,
                Shortname = larps.Shortname,
                Location = larps.Location,
            };

            currentLarp.Isactive = false;
            _context.Add(newLarp);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                    throw;
            }



            //_context.Entry(larps).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!LarpsExists(id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            return Ok(newLarp);
        }

        // POST: api/Larps
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Larps>> PostLarps(Larps larps)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
                if (!result.Result.roles.Contains("Wizard"))
            {
                return Unauthorized();
            }

            _context.Larps.Add(larps);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLarps", new { id = larps.Guid }, larps);
        }

        // DELETE: api/Larps/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Larps>> DeleteLarps(Guid id)
        {
            var larps = await _context.Larps.FindAsync(id);
            if (larps == null)
            {
                return NotFound();
            }

            //Change to deactivate and create new
            //_context.Larps.Remove(larps);
            //await _context.SaveChangesAsync();

            return larps;
        }

        private bool LarpsExists(Guid id)
        {
            return _context.Larps.Any(e => e.Guid == id);
        }
    }
}
