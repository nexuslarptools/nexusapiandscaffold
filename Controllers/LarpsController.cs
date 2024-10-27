using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LarpsController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public LarpsController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    // GET: api/Larps
    [HttpGet]
    public async Task<ActionResult<List<LARPOut>>> GetLarps()
    {
        var larpList = await _context.Larps.Where(l => l.Isactive == true)
            .Select(l => new LARPOut(l.Guid, l.Name, l.Shortname, l.Location, l.Isactive)).ToListAsync();

        return larpList.OrderBy(ll => ll.Name).ToList();
    }

    [HttpGet("Accessible")]
    public async Task<ActionResult<List<LARPOut>>> GetCurrUserLarps()
    {


          var  larpList = await _context.Larps.Where(l => l.Isactive == true)
            .Select(l => new LARPOut(l.Guid, l.Name, l.Shortname, l.Location, l.Isactive)).ToListAsync();

        return larpList.OrderBy(ll => ll.Name).ToList();
    }

    [HttpGet("GMAccess")]
    public async Task<ActionResult<List<LARPOut>>> GetLarpsWithGMAccess()
    {

            return await _context.Larps.Where(l =>
                    l.Isactive == true && l.Guid != Guid.Parse("0b247b46-86fd-11ed-956d-7faf2be673cc"))
                .Select(l => new LARPOut(l.Guid, l.Name, l.Shortname, l.Location, l.Isactive)).ToListAsync();

    }

    [HttpGet("WithGMs")]
    public async Task<ActionResult<List<LARPOut>>> GetLarpsWithAssignedGMs()
    {
        var larpList = await _context.Larps.Where(l => l.Isactive == true).ToListAsync();

        var larpoutput = new List<LARPOut>();

        foreach (var larp in larpList)
        {
            var newLarp = new LARPOut
            {
                Guid = larp.Guid,
                Name = larp.Name,
                Location = larp.Location,
                Isactive = larp.Isactive,
                Shortname = larp.Shortname,
                Users = await _context.Users
                    .Where(u => u.UserLarproles.Any(ulr => ulr.Larpguid == larp.Guid && ulr.Isactive == true))
                    .Select(u => new UserOut(u.Guid, u.Firstname, u.Lastname, u.Preferredname, u.Email, u.Pronounsguid, u.Pronouns.Pronouns,
                        u.Discordname, new RoleOut()
                    )).ToListAsync()
            };

            foreach (var user in newLarp.Users)
            {
                var topRole = new RoleOut();

                var userRoles = await _context.UserLarproles
                    .Where(ulr => ulr.Userguid == user.Guid && ulr.Larpguid == larp.Guid)
                    .Select(ulr => new RoleOut((int)ulr.Role.Ord, ulr.Role.Rolename)).ToListAsync();

                foreach (var role in userRoles)
                    if (topRole == null || topRole.RoleID < role.RoleID)
                        topRole = role;

                user.EffectiveRole = topRole;
            }

            larpoutput.Add(newLarp);
        }


        return larpoutput.OrderBy(lop => lop.Name).ToList();
    }


    // GET: api/Larps/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Larp>> GetLarps(Guid id)
    {
        var larps = await _context.Larps.FindAsync(id);

        if (larps == null) return NotFound();

        return larps;
    }

    // PUT: api/Larps/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    public async Task<ActionResult<Larps>> PutLarps(Guid guid, Larps larps)
    {

        var currentLarp = _context.Larps.Where(l => l.Isactive == true && l.Guid == guid).FirstOrDefault();

        if (currentLarp == null) return BadRequest();

        currentLarp.Name = larps.Name;
        currentLarp.Shortname = larps.Shortname;
        currentLarp.Location = larps.Location;
        currentLarp.Isactive = true;

        _context.Update(currentLarp);

        await _context.SaveChangesAsync();

        return Ok(currentLarp);
    }

    // POST: api/Larps
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    public async Task<ActionResult<Larp>> PostLarps(Larp larps)
    {
        _context.Larps.Add(larps);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetLarps", new { id = larps.Guid }, larps);
    }

    // DELETE: api/Larps/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<Larp>> DeleteLarps(Guid id)
    {

            var larps = await _context.Larps.FindAsync(id);
            if (larps == null) return NotFound();

            //Change to deactivate and create new
            //_context.Larps.Remove(larps);
            //await _context.SaveChangesAsync();

            larps.Isactive = false;
            await _context.SaveChangesAsync();


            return larps;
    }

    private bool LarpsExists(Guid id)
    {
        return _context.Larps.Any(e => e.Guid == id);
    }
}