using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly NexusLARPContextBase _context;

    public TagsController(NexusLARPContextBase context)
    {
        _context = context;
    }

    // GET: api/v1/Tags
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Tags>>> GetTags()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            return await _context.Tags.Where(t => t.Larptags.Any(lt => lt.Larpguid == null)).ToListAsync();
        return Unauthorized();
    }

    // GET: api/v1/Tags
    [HttpGet("groupbytypeRead")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TagsOutput>>> GetTagsGroupedByType()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var userTags = UsersLogic.GetUserTagsList(authId, _context, "Reader",
                UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context));

            var output = new List<TagsOutput>();
            var tagTypes = await _context.TagTypes.ToListAsync();

            foreach (var type in tagTypes)
            {
                var outp = new TagsOutput();
                outp.TagType = type.Name;

                var checkList = await _context.Tags.Where(t => t.Tagtypeguid == type.Guid && t.Isactive == true
                    && (t.Larptags.Any(lt => lt.Larpguid == null && lt.Isactive == true)
                        || userTags.Contains(t.Guid))).ToListAsync();

                var tagList = checkList.Select(tt => new outTag(tt.Name, tt.Guid, tt.Tagtypeguid, tt.Isactive, false))
                    .ToList();

                foreach (var usrtag in userTags)
                foreach (var tagL in tagList)
                    if (usrtag == tagL.Guid)
                    {
                        tagL.IsLocked = true;
                        tagL.LarpsTagLockedTo = _context.Larps.Where(l => l.Larptags
                                .Any(lt => lt.Tagguid == tagL.Guid && lt.Isactive == true))
                            .Select(l => new LARPOut(l.Guid, l.Name, l.Shortname, l.Location, l.Isactive)).ToList();
                    }


                outp.TagsList = tagList.OrderBy(x => x.Name).ToList();

                output.Add(outp);
            }

            return output;
        }

        return Unauthorized();
    }


    // GET: api/v1/Tags
    [HttpGet("groupbytypeWrite")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TagsOutput>>> GetTagsGroupedByTypeWrite()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var userTags = UsersLogic.GetUserTagsList(authId, _context, "Writer",
                UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context));

            var output = new List<TagsOutput>();
            var tagTypes = await _context.TagTypes.ToListAsync();

            foreach (var type in tagTypes)
            {
                var outp = new TagsOutput();
                outp.TagType = type.Name;

                var checkList = await _context.Tags.Where(t => t.Tagtypeguid == type.Guid && t.Isactive == true
                    && (t.Larptags.Any(lt => lt.Larpguid == null && lt.Isactive == true)
                        || userTags.Contains(t.Guid))).ToListAsync();

                var tagList = checkList.Select(tt => new outTag(tt.Name, tt.Guid, tt.Tagtypeguid, tt.Isactive, false))
                    .ToList();

                foreach (var usrtag in userTags)
                foreach (var tagL in tagList)
                    if (usrtag == tagL.Guid)
                    {
                        tagL.IsLocked = true;

                        tagL.LarpsTagLockedTo = _context.Larps.Where(l => l.Larptags
                                .Any(lt => lt.Tagguid == tagL.Guid && lt.Isactive == true))
                            .Select(l => new LARPOut(l.Guid, l.Name, l.Shortname, l.Location, l.Isactive)).ToList();
                    }

                outp.TagsList = tagList.OrderBy(x => x.Name).ToList();

                output.Add(outp);
            }

            return output;
        }

        return Unauthorized();
    }

    // GET: api/v1/Tags/5
    [HttpGet("{guid}")]
    [Authorize]
    public async Task<ActionResult<outTag>> GetTags(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var tags = await _context.Tags.Where(t => t.Guid == guid)
                .Select(t => new outTag(t.Name, t.Guid, t.Tagtypeguid, t.Isactive, false)).FirstOrDefaultAsync();

            if (tags == null) return NotFound();

            tags.LarpsTagLockedTo = _context.Larptags
                .Where(lt => lt.Tagguid == tags.Guid && lt.Larpguid != null && lt.Isactive == true)
                .Select(lt => new LARPOut(lt.Larpgu.Guid, lt.Larpgu.Name, lt.Larpgu.Shortname, lt.Larpgu.Location,
                    lt.Larpgu.Isactive)).ToList();

            if (tags.LarpsTagLockedTo.Count > 0) tags.IsLocked = true;

            return Ok(tags);
        }

        return Unauthorized();
    }

    // PUT: api/v1/Tags/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize]
    public async Task<IActionResult> PutTags(Guid guid, TagsInput tags)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            if (guid != tags.Guid) return BadRequest();

            var updateTag = await _context.Tags.Where(t => t.Guid == guid).FirstOrDefaultAsync();

            if (updateTag == null) return NotFound();

            if (tags.Name != string.Empty && tags.Name != null) updateTag.Name = tags.Name;

            if (tags.Tagtypeguid != null && _context.TagTypes.Any(tt => tt.Guid == tags.Tagtypeguid))
                updateTag.Tagtypeguid = tags.Tagtypeguid;

            var curTagLARPSActive = _context.Larptags.Where(lt => lt.Tagguid == updateTag.Guid && lt.Isactive == true)
                .Select(lt => lt.Larpguid).ToList();
            var curTagLARPSDeactive = _context.Larptags
                .Where(lt => lt.Tagguid == updateTag.Guid && lt.Isactive == false).Select(lt => lt.Larpguid).ToList();

            var checkfornull = _context.Larptags.Where(lt => lt.Tagguid == updateTag.Guid && lt.Larpguid == null)
                .FirstOrDefault();

            foreach (var inputLarpGuid in tags.LarptagGuid)
                if (_context.Larps.Any(l => l.Guid == inputLarpGuid))
                {
                    if (curTagLARPSDeactive.Contains(inputLarpGuid))
                    {
                        var updateLarpguid = _context.Larptags.Where(lt => lt.Tagguid == updateTag.Guid &&
                                                                           lt.Larpguid == inputLarpGuid
                                                                           && lt.Isactive == false).FirstOrDefault();

                        updateLarpguid.Isactive = true;
                        _context.Larptags.Update(updateLarpguid);
                        curTagLARPSActive.Add(inputLarpGuid);

                        if (checkfornull != null)
                        {
                            checkfornull.Isactive = false;
                            _context.Larptags.Update(checkfornull);
                        }
                    }

                    if (!curTagLARPSActive.Contains(inputLarpGuid))
                    {
                        var newLarpTag = new Larptags
                        {
                            Larpguid = inputLarpGuid,
                            Tagguid = updateTag.Guid,
                            Isactive = true
                        };

                        _context.Larptags.Add(newLarpTag);

                        if (checkfornull != null)
                        {
                            checkfornull.Isactive = false;
                            _context.Larptags.Update(checkfornull);
                        }
                    }
                }

            foreach (var activeGuid in curTagLARPSActive)
                if (activeGuid != null)
                    if (!tags.LarptagGuid.Contains((Guid)activeGuid))
                    {
                        var updateLarpguid = _context.Larptags.Where(lt => lt.Tagguid == updateTag.Guid &&
                                                                           lt.Larpguid == (Guid)activeGuid
                                                                           && lt.Isactive == true).FirstOrDefault();

                        if (checkfornull == null)
                        {
                            updateLarpguid.Larpguid = null;
                        }
                        else
                        {
                            updateLarpguid.Isactive = false;
                            checkfornull.Isactive = true;
                            _context.Larptags.Update(checkfornull);
                        }

                        _context.Larptags.Update(updateLarpguid);
                    }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagsExists(guid))
                    return NotFound();
                throw;
            }

            // var tagRet = new outTag(updateTag.Name, updateTag.Guid, updateTag.Tagtypeguid, updateTag.Isactive, false);

            // tagRet.LarpsTagLockedTo = _context.Larptags.Where(lt => lt.Tagguid == tags.Guid && lt.Larpguid != null)
            //  .Select(lt => new LARPOut(lt.Larpgu.Guid, lt.Larpgu.Name, lt.Larpgu.Shortname, lt.Larpgu.Location, lt.Larpgu.Isactive)).ToList();

            // if (tagRet.LarpsTagLockedTo.Count > 0)
            // {
            //     tagRet.IsLocked = true;
            //}

            return Ok();
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Returns all Tag Types and Guids
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Tags/
    [HttpGet("AllTagsByTypeName/{TypeName}")]
    [Authorize]
    public async Task<ActionResult<TagsOutput>> GetTagTypesWithTags(string TypeName)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var userTags = UsersLogic.GetUserTagsList(authId, _context, "Reader",
                UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context));
            var tagType = await _context.TagTypes.Where(tt => tt.Name == TypeName).FirstOrDefaultAsync();
            var Foundtags = await _context.Tags.Where(t => t.Tagtypegu.Guid == tagType.Guid && t.Isactive == true
                && (t.Larptags.Any(lt => lt.Larpguid == null)
                    || userTags.Contains(t.Guid))).ToListAsync();

            if (Foundtags == null) return NotFound();

            var outp = new TagsOutput();
            outp.TagType = tagType.Name;

            var tagList = Foundtags.Select(tt => new outTag(tt.Name, tt.Guid, tt.Tagtypeguid, tt.Isactive, false))
                .ToList();

            foreach (var usrtag in userTags)
            foreach (var tagL in tagList)
                if (usrtag == tagL.Guid)
                    tagL.IsLocked = true;

            outp.TagsList = tagList.OrderBy(x => x.Name).ToList();
            return Ok(outp);
        }

        return Unauthorized();
    }

    // GET: api/LinkedCharactersAndItems/5
    [HttpGet("LinkedCharactersAndItems/{guid}")]
    [Authorize]
    public async Task<ActionResult<ListAllTagOptions>> GetAllCharactersAndItemsLinkedSeries(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try
            {
                ListAllTagOptions output = new ListAllTagOptions();

                var tag = await _context.Tags.Where(s => s.Isactive == true && s.Guid == guid).FirstOrDefaultAsync();

                if (tag == null)
                {
                    return BadRequest("Tag Not Found");
                }

                var typeGu = await _context.TagTypes.Where(s => s.Guid == tag.Tagtypeguid).FirstOrDefaultAsync();

                if (typeGu.Name == "Item" || typeGu.Name == "LARPRun")
                {

                    var itemList = _context.ItemSheet.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var itemSheet in itemList)
                    {
                        var tagList = new JsonElement();
                        itemSheet.Fields.RootElement.TryGetProperty("Tags", out tagList);

                        if (tagList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                            foreach (var listtag in TestJsonFeilds)
                            {
                                if (listtag.ToString() == guid.ToString())
                                {
                                    output.ItemLists.AddUnapproved(itemSheet);
                                }
                            }
                        }
                    }

                    var approvitemList = _context.ItemSheetApproved.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var itemSheet in approvitemList)
                    {
                        var startitemsList = new JsonElement();
                        itemSheet.Fields.RootElement.TryGetProperty("Tags", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                            foreach (var listtag in TestJsonFeilds)
                            {
                                if (listtag.ToString() == guid.ToString())
                                {
                                    output.ItemLists.AddApproved(itemSheet);
                                }
                            }
                        }
                    }
                }

                if (typeGu.Name == "Character" || typeGu.Name == "LARPRun")
                {

                    var charList = _context.CharacterSheet.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var characterSheet in charList)
                    {
                        var startitemsList = new JsonElement();
                        characterSheet.Fields.RootElement.TryGetProperty("Tags", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                            foreach (var listtag in TestJsonFeilds)
                            {
                                if (listtag.ToString() == guid.ToString())
                                {
                                    output.CharacterLists.AddUnapproved(characterSheet);
                                }
                            }
                        }
                    }

                    var approvcharList = _context.CharacterSheetApproved.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var characterSheet in approvcharList)
                    {
                        var startitemsList = new JsonElement();
                        characterSheet.Fields.RootElement.TryGetProperty("Tags", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Tags").EnumerateArray();

                            foreach (var listtag in TestJsonFeilds)
                            {
                                if (listtag.ToString() == guid.ToString())
                                {
                                    output.CharacterLists.AddApproved(characterSheet);
                                }
                            }
                        }
                    }
                }

                if (typeGu.Name == "Ability" || typeGu.Name == "LARPRun")
                {

                    var charList = _context.CharacterSheet.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var characterSheet in charList)
                    {
                        var startitemsList = new JsonElement();
                        characterSheet.Fields.RootElement.TryGetProperty("Special_Skills", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = startitemsList.EnumerateArray();

                            foreach (var skill in TestJsonFeilds)
                            {
                                var skilltaglist = new JsonElement();
                                skill.TryGetProperty("Tags", out skilltaglist);

                                if (skilltaglist.ValueKind.ToString() != "Undefined")
                                {
                                    var listSkillTags = skilltaglist.EnumerateArray();

                                    foreach (var skilltag in listSkillTags)
                                    {
                                        if (skilltag.ToString() == guid.ToString() && !output.CharacterLists.UnapprovedContainsGuid(guid))
                                        {
                                            output.CharacterLists.AddUnapproved(characterSheet);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var approvcharList = _context.CharacterSheetApproved.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var characterSheet in approvcharList)
                    {
                        var startitemsList = new JsonElement();
                        characterSheet.Fields.RootElement.TryGetProperty("Special_Skills", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = characterSheet.Fields.RootElement.GetProperty("Special_Skills").EnumerateArray();

                            foreach (var skill in TestJsonFeilds)
                            {

                                var skilltaglist = new JsonElement();
                                skill.TryGetProperty("Tags", out skilltaglist);

                                if (skilltaglist.ValueKind.ToString() != "Undefined")
                                {
                                    var listSkillTags = skilltaglist.EnumerateArray();

                                    foreach (var skilltag in listSkillTags)
                                    {
                                        if (skilltag.ToString() == guid.ToString() && !output.CharacterLists.ApprovedContainsGuid(guid))
                                        {
                                            output.CharacterLists.AddApproved(characterSheet);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var itemList = _context.ItemSheet.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var itemSheet in itemList)
                    {
                        var startitemsList = new JsonElement();
                        itemSheet.Fields.RootElement.TryGetProperty("Special_Skills", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Special_Skills").EnumerateArray();

                            foreach (var skill in TestJsonFeilds)
                            {

                                var skilltaglist = new JsonElement();
                                skill.TryGetProperty("Tags", out skilltaglist);

                                if (skilltaglist.ValueKind.ToString() != "Undefined")
                                {
                                    var listSkillTags = skilltaglist.EnumerateArray();

                                    foreach (var skilltag in listSkillTags)
                                    {
                                        if (skilltag.ToString() == guid.ToString() && !output.ItemLists.UnapprovedContainsGuid(guid))
                                        {
                                            output.ItemLists.AddUnapproved(itemSheet);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var approvitemList = _context.ItemSheetApproved.Where(cs => cs.Isactive == true).OrderBy(i => i.Name).ToList();

                    foreach (var itemSheet in approvitemList)
                    {
                        var startitemsList = new JsonElement();
                        itemSheet.Fields.RootElement.TryGetProperty("Special_Skills", out startitemsList);

                        if (startitemsList.ValueKind.ToString() != "Undefined")
                        {
                            var TestJsonFeilds = itemSheet.Fields.RootElement.GetProperty("Special_Skills").EnumerateArray();

                            foreach (var skill in TestJsonFeilds)
                            {

                                var skilltaglist = new JsonElement();
                                skill.TryGetProperty("Tags", out skilltaglist);

                                if (skilltaglist.ValueKind.ToString() != "Undefined")
                                {
                                    var listSkillTags = skilltaglist.EnumerateArray();

                                    foreach (var skilltag in listSkillTags)
                                    {
                                        if (skilltag.ToString() == guid.ToString() && !output.ItemLists.ApprovedContainsGuid(guid))
                                        {
                                            output.ItemLists.AddApproved(itemSheet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if (typeGu.Name == "Series" || typeGu.Name == "LARPRun")
                {
                    var seriesList = _context.Series.Where(cs => cs.Isactive == true).ToList().OrderBy(cs => cs.Title);

                    foreach (var series in seriesList)
                    {
                        var startitemsList = new JsonElement();

                        if (series.Tags != null)
                        {
                            series.Tags.RootElement.TryGetProperty("SeriesTags", out startitemsList);

                            if (startitemsList.ValueKind.ToString() != "Undefined")
                            {
                                var TestJsonFeilds = series.Tags.RootElement.GetProperty("SeriesTags").EnumerateArray();

                                foreach (var listtag in TestJsonFeilds)
                                {
                                    if (listtag.ToString() == guid.ToString())
                                    {
                                        output.SeriesList.Add(series);
                                    }
                                }
                            }
                        }
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        return Unauthorized();
    }

    // POST: api/v1/Tags
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Tags>> PostTags(TagsInput tags)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context) ||
            UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context))
        {
            var currUser = await _context.Users.Where(u => u.Authid == authId).FirstOrDefaultAsync();

            if (!_context.TagTypes.Any(tt => tt.Guid == tags.Tagtypeguid)) return BadRequest();

            var newTag = new Tags
            {
                Guid = Guid.NewGuid(),
                Name = tags.Name,
                Tagtypeguid = tags.Tagtypeguid,
                Isactive = true
            };

            _context.Tags.Add(newTag);

            if (tags.LarptagGuid == null || tags.LarptagGuid.Count == 0)
            {
                var tagLarp = new Larptags
                {
                    Tagguid = newTag.Guid
                };

                _context.Larptags.Add(tagLarp);
            }
            else
            {
                foreach (var larpguid in tags.LarptagGuid)
                {
                    if (!UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
                        if (!_context.Larps.Any(tt => tt.Guid == larpguid) ||
                            (UsersLogic.IsUserAuthed(authId, accessToken, "HeadGM", _context)
                             && !_context.UserLarproles.Any(ulr => ulr.Larpguid == larpguid
                                                                   && ulr.Isactive == true
                                                                   && ulr.Userguid == currUser.Guid
                                                                   && ulr.Roleid > 3)))
                            return BadRequest();

                    var tagLarp = new Larptags
                    {
                        Tagguid = newTag.Guid,
                        Larpguid = larpguid
                    };

                    _context.Larptags.Add(tagLarp);
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("PostTags", new { guid = tags.Guid }, tags);
        }

        return Unauthorized();
    }

    // DELETE: api/v1/Tags/5
    [HttpDelete("{guid}")]
    [Authorize]
    public async Task<ActionResult<Tags>> DeleteTags(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var tags = await _context.Tags.Where(t => t.Guid == guid).FirstOrDefaultAsync();
            if (tags == null) return NotFound();

            tags.Isactive = false;

            _context.Tags.Update(tags);
            _context.SaveChanges();

            return tags;
        }

        return Unauthorized();
    }

    private bool TagsExists(Guid id)
    {
        return _context.Tags.Any(e => e.Guid == id);
    }
}