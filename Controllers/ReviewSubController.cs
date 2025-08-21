using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReviewSubController : ControllerBase
    {
        private readonly NexusLarpLocalContext _context;

        public ReviewSubController(NexusLarpLocalContext context)
        {
            _context = context;
        }


        //Get
        [HttpGet("Item/IsSubbbed/{guid}")]
        [Authorize]
        public async Task<ActionResult<ReviewMessageSubscription>> GetIsItemSubscribed(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);

                var itemResults = await rmsl.GetItemMessageSubsByItem(guid);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);


                if (itemResults.Where(ir => ir.UserGuid == currUserGuid && ir.Stopdate == null).FirstOrDefault() != null)
                {
                    return Ok(itemResults.Where(ir => ir.UserGuid == currUserGuid && ir.Stopdate == null).FirstOrDefault());
                }

                return Ok(new ReviewMessageSubscription());
            }

            return Unauthorized();
        }

        [HttpGet("Character/IsSubbbed/{guid}")]
        [Authorize]
        public async Task<ActionResult<ReviewMessageSubscription>> GetIsCharacterSubscribed(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);

                var cResults = await rmsl.GetCharacterMessageSubsByCharacter(guid);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);


                if (cResults.Where(ir => ir.UserGuid == currUserGuid && ir.Stopdate == null).FirstOrDefault() != null)
                {
                    return Ok(cResults.Where(ir => ir.UserGuid == currUserGuid && ir.Stopdate == null).FirstOrDefault());
                }

                return Ok(new ReviewMessageSubscription());
            }

            return Unauthorized();
        }

        //Get Sub
        // GET: api/v1/ReviewSub
        [HttpGet("Item/{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewMessageSubscription>> GetItemSubscription(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rmsl.GetItemMessageSub(id);
                return Ok(new ReviewMessageSubscription(result, _context));
            }
            return Unauthorized();
        }

        [HttpGet("Character/{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewMessageSubscription>> GetCharacterSubscription(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rmsl.GetCharacterMessageSub(id);
                return Ok(new ReviewMessageSubscription(result, _context));
            }
            return Unauthorized();
        }

        //Get All subs for user
        [HttpGet("AllItemSubs")]
        [Authorize]
        public async Task<ActionResult<List<ReviewMessageSubscription>>> GetItemAllSubscriptions()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var results = await rmsl.GetItemMessageSubsByuser(currUserGuid);

                return Ok(results);
            }
            return Unauthorized();
        }

        [HttpGet("AllCharacterSubs")]
        [Authorize]
        public async Task<ActionResult<ReviewMessageSubscription>> GetAllCharacterSubscriptions()
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var results = await rmsl.GetCharacterMessageSubsByUser(currUserGuid);

                return Ok(results);
            }
            return Unauthorized();
        }

        //Get All subs for itemsheet
        [HttpGet("AllItemSub/{guid}")]
        [Authorize]
        public async Task<ActionResult<List<ReviewMessageSubscription>>> GetSubscriptionsForItem(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);

                var results = await rmsl.GetItemMessageSubsByItem(guid);

                return Ok(results);
            }
            return Unauthorized();
        }

        //Get All subs for charactersheet
        [HttpGet("AllCharacterSub/{guid}")]
        [Authorize]
        public async Task<ActionResult<List<ReviewMessageSubscription>>> GetSubscriptionsForCharacter(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);

                var results = await rmsl.GetCharacterMessageSubsByCharacter(guid);

                return Ok(results);
            }
            return Unauthorized();
        }

        //Add Sub
        [HttpPost("Item")]
        [Authorize]
        public async Task<ActionResult<int>> NewSubscriptionForItem(
            [FromBody] ReviewMessageSubscription itemSub)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                itemSub.Createdate = DateTime.Now;
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                itemSub.UserGuid = currUserGuid;
                itemSub.Stopdate = null;

                var rmsl = new ReviewMessageSubsLogic(_context);

                var results = await rmsl.AddItemMessageSub(itemSub);

                return Ok(results);
            }
            return Unauthorized();
        }

        [HttpPost("Character")]
        [Authorize]
        public async Task<ActionResult<int>> NewSubscriptionForCharacter(
                        [FromBody] ReviewMessageSubscription characterSub)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);

                characterSub.Createdate = DateTime.Now;
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                characterSub.UserGuid = currUserGuid;
                characterSub.Stopdate = null;

                var results = await rmsl.AddCharacterMessageSub(characterSub);

                return Ok(results);
            }
            return Unauthorized();
        }

        //Stop Sub
        [HttpPut("StopItemSub/{id}")]
        [Authorize]
        public async Task<ActionResult<bool>> StopSubscriptionForItem(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);
                var result = await rmsl.GetItemMessageSub(id);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                if (!UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) && currUserGuid != result.UserGuid)
                {
                    return Unauthorized();
                }

                var output = await rmsl.StopItemMessageSub(id);
                return Ok(output);
            }
            return Unauthorized();
        }

        [HttpPut("StopCharacterSub/{id}")]
        [Authorize]
        public async Task<ActionResult<bool>> StopSubscriptionForCharacter(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {

                var rmsl = new ReviewMessageSubsLogic(_context);
                var result = await rmsl.GetCharacterMessageSub(id);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                if (!UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) && currUserGuid != result.UserGuid)
                {
                    return Unauthorized();
                }

                var output = await rmsl.StopCharacterMessageSub(id);

                return Ok(output);
            }
            return Unauthorized();
        }

        //Delete Sub
        [HttpDelete("DeleteItemSub/{id}")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteSubscriptionForItem(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {

                var rmsl = new ReviewMessageSubsLogic(_context);
                var result = await rmsl.DeleteItemMessageSub(id);

                return Ok(result);
            }
            return Unauthorized();
        }

        [HttpDelete("DeleteCharacterSub/{id}")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteSubscriptionForCharacter(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rmsl = new ReviewMessageSubsLogic(_context);
                var result = await rmsl.DeleteCharacterMessageSub(id);

                return Ok(result);
            }
            return Unauthorized();
        }

    }
}
