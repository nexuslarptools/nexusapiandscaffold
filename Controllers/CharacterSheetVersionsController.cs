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
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CharacterSheetVersionsController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public CharacterSheetVersionsController(NexusLARPContextBase context)
        {
            _context = context;
        }
        /// <summary>
        /// Get all character sheet versions.
        /// </summary>
        /// <returns></returns>
        // GET: api/CharacterSheetVersions
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheetVersion>>> GetCharacterSheetVersion()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBApprover"))
            {
                var sheetVersionInfo = await _context.CharacterSheetVersion.Select(csv => new
                {
                    csv.Id,
                    csv.Guid,
                    csv.Version,
                    csv.Name,
                    csv.Seriesguid,
                    csv.Seriesgu.Title,
                    csv.Createdate,
                    Createdby = csv.CreatedbyuserGu.Firstname + " " + csv.CreatedbyuserGu.Lastname,
                    CreatedbyEmail = csv.CreatedbyuserGu.Email,
                    csv.Reason4edit
                }).ToListAsync();

                return Ok(sheetVersionInfo);

            }
            return Unauthorized();
        }

        /// <summary>
        /// Get all character sheet versions for a single character
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/CharacterSheetVersions
        [HttpGet("character/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CharacterSheetVersion>>> GetCharacterSheetVersionByGuid(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBApprover"))
            {
                var sheetVersionInfo = await _context.CharacterSheetVersion.Where(c => c.Guid == guid).Select(csv => new
                {
                    csv.Id,
                    csv.Guid,
                    csv.Version,
                    csv.Name,
                    csv.Seriesguid,
                    csv.Seriesgu.Title,
                    csv.Createdate,
                    Createdby = csv.CreatedbyuserGu.Firstname + " " + csv.CreatedbyuserGu.Lastname,
                    CreatedbyEmail = csv.CreatedbyuserGu.Email,
                    csv.Reason4edit
                }).ToListAsync();

                return Ok(sheetVersionInfo);

            }
            return Unauthorized();
        }


        /// <summary>
        /// Get the sheet versions that exist by id in versions, a sigular sheet.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/CharacterSheetVersions/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CharacterSheetVersion>> GetCharacterSheetVersion(int id)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBApprover"))
            {
                var characterSheetVersion = await _context.CharacterSheetVersion.FindAsync(id);

                if (characterSheetVersion == null)
                {
                    return NotFound();
                }

                var outputsheet = Character.CreateCharSheet(characterSheetVersion);


                return Ok(outputsheet);
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
        public async Task<IActionResult> RevertCharacterSheetVersion(int id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var oldSheet = await _context.CharacterSheetVersion.Where(csv => csv.Id == id).FirstOrDefaultAsync();

                if (oldSheet == null)
                {
                    return BadRequest();
                }


                var editSheet = await _context.CharacterSheet.Where(cs => cs.Guid == oldSheet.Guid).FirstOrDefaultAsync();

                editSheet.Seriesguid = oldSheet.Seriesguid;
                editSheet.Name = oldSheet.Name;
                editSheet.Img1 = oldSheet.Img1;
                editSheet.Img2 = oldSheet.Img2;
                editSheet.Fields = JsonDocument.Parse(oldSheet.Fields.RootElement.ToString());
                editSheet.Isactive = true;
                editSheet.Createdate = DateTime.Now;
                editSheet.CreatedbyuserGuid = result.Result.userGuid;
                editSheet.FirstapprovalbyuserGuid = null;
                editSheet.Firstapprovaldate = null;
                editSheet.SecondapprovalbyuserGuid = null;
                editSheet.Secondapprovaldate = null;
                editSheet.Gmnotes = null;
                editSheet.Reason4edit = null;

                _context.CharacterSheet.Update(editSheet);
                await _context.SaveChangesAsync();

                return Ok(editSheet);

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
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCharacterSheetVersion(int id, CharacterSheetVersion characterSheetVersion)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                if (id != characterSheetVersion.Id)
                {
                    return BadRequest();
                }

                _context.Entry(characterSheetVersion).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CharacterSheetVersionExists(id))
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


        // POST: api/CharacterSheetVersions
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CharacterSheetVersion>> PostCharacterSheetVersion(CharacterSheetVersion characterSheetVersion)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                _context.CharacterSheetVersion.Add(characterSheetVersion);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCharacterSheetVersion", new { id = characterSheetVersion.Id }, characterSheetVersion);
            }
            return Unauthorized();
        }

        // DELETE: api/CharacterSheetVersions/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<CharacterSheetVersion>> DeleteCharacterSheetVersion(int id)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersController.GetUserInfo(accessToken);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {
                var characterSheetVersion = await _context.CharacterSheetVersion.FindAsync(id);
                if (characterSheetVersion == null)
                {
                    return NotFound();
                }

                _context.CharacterSheetVersion.Remove(characterSheetVersion);
                await _context.SaveChangesAsync();

                return characterSheetVersion;
            }
            return Unauthorized();
        }

        private bool CharacterSheetVersionExists(int id)
        {
            return _context.CharacterSheetVersion.Any(e => e.Id == id);
        }
    }
}
