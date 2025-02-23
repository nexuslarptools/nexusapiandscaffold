using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CharacterSheetReviewMessageController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public CharacterSheetReviewMessageController(NexusLarpLocalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewMessage>>> Get()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var isheetReviews = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Isactive == true).ToList();

            var output = new List<ReviewMessage>();

            foreach (var message in isheetReviews) output.Add(new ReviewMessage(message, _context));

            return Ok(output);
        }

        return Unauthorized();
    }

    // GET api/<CharacterSheetReviewMessageController>/5
    [HttpGet("{itemSheetId}")]
    public async Task<ActionResult<IEnumerable<ReviewMessage>>> GetAllForItem(int itemSheetId)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var csheetReviews = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Isactive == true
                && isrm.CharactersheetId == itemSheetId).ToList();

            var output = new List<ReviewMessage>();

            foreach (var message in csheetReviews)
            {
                output.Add(new ReviewMessage(message, _context));
            }

            return Ok(output);
        }

        return Unauthorized();
    }

    // POST api/<CharacterSheetReviewMessageController>
    [HttpPost]
    [DisableRequestSizeLimit]
    [Authorize]
    public async Task<IActionResult> PostItemReviewMessage([FromBody] ReviewMessage reviewMessage)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var newsheet = reviewMessage.ConvertToCharacterSheetMessage();
            var result = _context.Users.Where(u => u.Authid == authId).Select(u => u.Guid).FirstOrDefault();
            newsheet.CreatedbyuserGuid = result;
            _context.CharacterSheetReviewMessages.Add(newsheet);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        return Unauthorized();
    }

    // PUT api/<CharacterSheetReviewMessageController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ReviewMessage reviewMessage)
    {
        if (id != reviewMessage.Id) return BadRequest();

        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var currMessage = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Id == id).FirstOrDefault();

            if (currMessage != null)
            {
                currMessage = reviewMessage.ConvertToCharacterSheetMessage();
                _context.CharacterSheetReviewMessages.Update(currMessage);

                await _context.SaveChangesAsync();

                return NoContent();
            }

            return BadRequest();
        }

        return Unauthorized();
    }


    // DELETE api/<CharacterSheetReviewMessageController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Approver", _context))
        {
            var currMessage = _context.CharacterSheetReviewMessages
                .Where(isrm => isrm.Id == id && isrm.Isactive == true).FirstOrDefault();

            if (currMessage != null)
            {
                currMessage.Isactive = false;
                _context.CharacterSheetReviewMessages.Update(currMessage);

                await _context.SaveChangesAsync();

                return NoContent();
            }

            return BadRequest();
        }

        return Unauthorized();
    }
}