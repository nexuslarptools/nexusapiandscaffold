using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
    public class ReviewMessagesController : ControllerBase
    {
        private readonly NexusLarpLocalContext _context;

        public ReviewMessagesController(NexusLarpLocalContext context)
        {
            _context = context;
        }


        // GET: api/v1/ReviewMessages
        [HttpGet("ItemId/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewMessage>>> GetItemReviewMessages(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var rmal = new ReviewMessageAcksLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetItemMessage(id);
                var outp = new ReviewMessage(result, _context);

                if (outp.CreatedbyuserGuid == currUserGuid)
                {
                    outp.IsEditable = true;
                }

                var allMessageacks = await rmal.GetAllUnseenItemMessageAcksForUser(currUserGuid);
                var foundMessageack = allMessageacks.Where(ma => ma.MessageId == outp.Id).FirstOrDefault();

                if (foundMessageack != null)
                {
                    await rmal.MarkSeenItemMessageAck(foundMessageack);
                }

                return Ok(outp);
            }
            return Unauthorized();

        }

        [HttpGet("CharacterId/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewMessage>>> GetCharacterReviewMessages(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var rmal = new ReviewMessageAcksLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                var usersList = _context.Users.Select(x => x).ToList();

                var result = await rml.GetCharacterMessage(id);
                var outp = new ReviewMessage(result, usersList);

                if (outp.CreatedbyuserGuid == currUserGuid)
                {
                    outp.IsEditable = true;
                }

                var allMessageacks = await rmal.GetAllUnseenCharacterMessageAcksForUser(currUserGuid);
                var foundMessageack = allMessageacks.Where(ma => ma.MessageId == outp.Id).FirstOrDefault();

                if (foundMessageack != null)
                {
                    await rmal.MarkSeenCharacterMessageAck(foundMessageack);
                }

                return Ok(outp);
            }
            return Unauthorized();

        }

        [HttpGet("Item/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewMessage>>> GetItemReviewMessages(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var rmal = new ReviewMessageAcksLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetAllActiveItemMessagesforItem(guid);
                var allMessageacks = await rmal.GetAllUnseenItemMessageAcksForUser(currUserGuid);

                foreach (var message in result)
                {
                    message.Acks = await rmal.GetAllItemMessageAcksForMessageByUser(message.Id, currUserGuid);
                    if (message.CreatedbyuserGuid == currUserGuid)
                    {
                        message.IsEditable = true;
                    }

                    var foundMessageack = allMessageacks.Where(ma => ma.MessageId == message.Id).FirstOrDefault();

                    if (foundMessageack != null)
                    {
                        await rmal.MarkSeenItemMessageAck(foundMessageack);
                    }
                }
                return Ok(result.OrderBy(r => r.Createdate));
            }
            return Unauthorized();

        }

        [HttpGet("Character/{guid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewMessage>>> GetCharacterReviewMessages(Guid guid)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var rmal = new ReviewMessageAcksLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetAllActiveCharacterMessagesforCharacter(guid);
                var allMessageacks = await rmal.GetAllUnseenCharacterMessageAcksForUser(currUserGuid);


                foreach (var message in result)
                {
                    message.Acks = await rmal.GetAllCharacterMessageAcksForMessageByUser(message.Id, currUserGuid);
                    if (message.CreatedbyuserGuid == currUserGuid)
                    {
                        message.IsEditable = true;
                    }

                    var foundMessageack = allMessageacks.Where(ma => ma.MessageId == message.Id).FirstOrDefault();

                    if (foundMessageack != null)
                    {
                        await rmal.MarkSeenCharacterMessageAck(foundMessageack);
                    }

                }
                return Ok(result.OrderBy(r => r.Createdate));
            }
            return Unauthorized();

        }

        [HttpPost("Item")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> PostItemReviewMessage([FromBody] ReviewMessage itemReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var rmsl = new ReviewMessageSubsLogic(_context);
                var rmal = new ReviewMessageAcksLogic(_context); //Have to add new acks based on subscription.
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                itemReview.CreatedbyuserGuid = currUserGuid;
                itemReview.Createdate = DateTime.Now;
                itemReview.IsActive = true;

                int result = await rml.AddItemMessage(itemReview);

                if (result > 0)
                {
                    var item = _context.ItemSheets.Where(ish => ish.Id == itemReview.SheetId).FirstOrDefault();
                    if (item != null)
                    {
                        var userSubs = await rmsl.GetItemMessageSubsByItem(item.Guid);

                        if (userSubs.Where(us => us.UserGuid == currUserGuid).FirstOrDefault() == null)
                        {
                            ReviewMessageSubscription newsub = new ReviewMessageSubscription()
                            {
                                UserGuid = currUserGuid,
                                SheetGuid = item.Guid,
                                Createdate = DateTime.Now,
                                Stopdate = null

                            };
                            int newSubResult = await rmsl.AddItemMessageSub(newsub);

                            await rmal.AddItemMessageAck(new MessageAck()
                            {
                                MessageId = result,
                                UserGuid = currUserGuid,
                                Isactive = true

                            });
                        }

                        foreach (var sub in userSubs)
                        {
                            await rmal.AddItemMessageAck(new MessageAck()
                            {
                                MessageId = result,
                                UserGuid = sub.UserGuid,
                                Isactive = true

                            });
                        }
                    }
                    itemReview.Id = result;
                    return Ok(itemReview);
                }
            }

            return Unauthorized();
        }

        [HttpPost("Character")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> PostCharacterReviewMessage([FromBody] ReviewMessage characterReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var rmsl = new ReviewMessageSubsLogic(_context);
                var rmal = new ReviewMessageAcksLogic(_context); //Have to add new acks based on subscription.
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);
                characterReview.CreatedbyuserGuid = currUserGuid;
                characterReview.Createdate = DateTime.Now;
                characterReview.IsActive = true;

                int result = await rml.AddCharacterMessage(characterReview);

                if (result > 0)
                {
                    var character = _context.CharacterSheets.Where(ish => ish.Id == characterReview.SheetId).FirstOrDefault();
                    if (character != null)
                    {
                        var userSubs = await rmsl.GetCharacterMessageSubsByCharacter(character.Guid);

                        if (userSubs.Where(us => us.UserGuid == currUserGuid).FirstOrDefault() == null)
                        {
                            ReviewMessageSubscription newsub = new ReviewMessageSubscription()
                            {
                                UserGuid = currUserGuid,
                                SheetGuid = character.Guid,
                                Createdate = DateTime.Now,
                                Stopdate = null

                            };
                            int newSubResult = await rmsl.AddCharacterMessageSub(newsub);

                            await rmal.AddCharacterMessageAck(new MessageAck()
                            {
                                MessageId = result,
                                UserGuid = currUserGuid,
                                Isactive = true

                            });
                        }
                        foreach (var sub in userSubs)
                        {
                            await rmal.AddCharacterMessageAck(new MessageAck()
                            {
                                MessageId = result,
                                UserGuid = sub.UserGuid,
                                Isactive = true

                            });
                        }
                    }
                    characterReview.Id = result;
                    return Ok(characterReview);
                }
            }

            return Unauthorized();
        }

        [HttpPut("Item/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> PutItemReviewMessage(int id, [FromBody] ReviewMessage itemReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context) && id == itemReview.Id)
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetItemMessage(id);
                if (currUserGuid != result.CreatedbyuserGuid && !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    return Unauthorized();
                }

                result.Message = itemReview.Message;
                result.ItemsheetId = itemReview.SheetId;
                await _context.SaveChangesAsync();

                return Ok(itemReview);

            }


            return Unauthorized();
        }

        [HttpPut("Character/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> PutCharacterReviewMessage(int id, [FromBody] ReviewMessage characterReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context) && id == characterReview.Id)
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetCharacterMessage(id);
                if (currUserGuid != result.CreatedbyuserGuid && !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    return Unauthorized();
                }

                result.Message = characterReview.Message;
                result.CharactersheetId = characterReview.SheetId;
                await _context.SaveChangesAsync();

                return Ok(characterReview);

            }

            return Unauthorized();
        }


        [HttpPut("Item/Deactivate/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult> RemoveItemReviewMessage(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetItemMessage(id);
                if (currUserGuid != result.CreatedbyuserGuid && !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    return Unauthorized();
                }

                result.Isactive = false;
                await _context.SaveChangesAsync();

                return Ok();

            }


            return Unauthorized();
        }

        [HttpPut("Character/Deactivate/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> RemoveCharacterReviewMessage(int id)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetCharacterMessage(id);
                if (currUserGuid != result.CreatedbyuserGuid && !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    return Unauthorized();
                }

                result.Isactive = false;
                await _context.SaveChangesAsync();

                return Ok();

            }

            return Unauthorized();
        }

        [HttpPut("Item/Reactivate/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> ReactivateItemReviewMessage(int id, [FromBody] ReviewMessage itemReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context) && id == itemReview.Id)
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetItemMessage(id);
                if (currUserGuid != result.CreatedbyuserGuid && !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    return Unauthorized();
                }

                result.Isactive = true;
                await _context.SaveChangesAsync();

                return Ok(itemReview);

            }


            return Unauthorized();
        }

        [HttpPut("Character/Reactivate/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> ReactivateCharacterReviewMessage(int id, [FromBody] ReviewMessage characterReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context) && id == characterReview.Id)
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.GetCharacterMessage(id);
                if (currUserGuid != result.CreatedbyuserGuid && !UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                {
                    return Unauthorized();
                }

                result.Isactive = true;
                await _context.SaveChangesAsync();

                return Ok(characterReview);

            }

            return Unauthorized();
        }


        [HttpDelete("Item/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> DeleteItemReviewMessage(int id, [FromBody] ReviewMessage itemReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) && id == itemReview.Id)
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.DeleteItemMessage(id);
                return Ok();

            }


            return Unauthorized();
        }

        [HttpDelete("Character/{id}")]
        [DisableRequestSizeLimit]
        [Authorize]
        public async Task<ActionResult<ReviewMessage>> DeleteCharacterReviewMessage(int id, [FromBody] ReviewMessage characterReview)
        {
            var authId = HttpContext.User.Claims.ToList()[1].Value;

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

            if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) && id == characterReview.Id)
            {
                var rml = new ReviewMessageLogic(_context);
                var currUserGuid = await UsersLogic.GetUserGuid(authId, _context);

                var result = await rml.DeleteCharacterMessage(id);
                return Ok(characterReview);

            }

            return Unauthorized();
        }

    }
}
