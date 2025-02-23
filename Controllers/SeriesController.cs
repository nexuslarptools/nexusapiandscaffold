using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

//using System.Web.Http;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SeriesController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;

    public SeriesController(NexusLarpLocalContext context)
    {
        _context = context;
    }


    /// <summary>
    ///     This gives full information for all series which exist in the database, including the null series.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Series
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Series>>> GetSeries()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var ser = await _context.Series
                .Where(s => s.Isactive == true && allowedSeries.Contains(s.Guid) && s.Title != "")
                .Select(s => new
                {
                    Series = s,
                    Sheets = _context.CharacterSheetApproveds.Where(csa => csa.Isactive == true
                                                                           && csa.Seriesguid == s.Guid).ToList()
                })
                .ToListAsync();

            var outputSeries = new List<Seri>();

            if (ser == null) return NotFound();

            var allTagsList = _context.Tags.ToList();

            foreach (var s in ser)
            {
                var newser = new Seri();

                newser.Guid = s.Series.Guid;
                newser.Title = s.Series.Title;
                newser.Titlejpn = s.Series.Titlejpn;
                newser.Createdate = s.Series.Createdate;
                newser.SheetTotal = s.Sheets.Count();
                newser.Tags = new List<TagOut>();

                if (s.Series.Tags != null)
                {
                    var taglist = JObject.Parse(s.Series.Tags.RootElement.ToString());
                    foreach (var tag in taglist["SeriesTags"])
                        newser.Tags.Add(new TagOut(allTagsList.Where(atl => atl.Guid == (Guid)tag).FirstOrDefault()));
                }

                outputSeries.Add(newser);
            }


            return Ok(outputSeries.OrderBy(o => StringLogic.IgnorePunct(o.Title)));
        }

        return Unauthorized();
    }


    [HttpGet("FullListWithApprovedChar")]
    [Authorize]
    public async Task<ActionResult<List<SeriWithCharSheets>>> GetSeriesWithApprovedCharList()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var ser = await _context.Series.Where(s => s.Isactive == true && allowedSeries.Contains(s.Guid) && s.Title != "")
                .OrderBy(o => StringLogic.IgnorePunct(o.Title))
                .Select(s => new
                {
                    Series = s,
                    Sheets = _context.CharacterSheetApproveds.Where(csa => csa.Isactive == true
                    && csa.Seriesguid == s.Guid).OrderBy(csa => StringLogic.IgnorePunct(csa.Name)).ToList()
                })
                .ToListAsync();

            ser = ser.Where(s => s.Sheets != null && s.Sheets.Count > 0).ToList();

            var outputSeries = new List<SeriWithCharSheets>();

            if (ser == null) return NotFound();

            var allTagsList = _context.Tags.ToList();

            foreach (var s in ser)
            {
                var newser = new SeriWithCharSheets();

                newser.Guid = s.Series.Guid;
                newser.Title = s.Series.Title;
                newser.Titlejpn = s.Series.Titlejpn;
                newser.Createdate = s.Series.Createdate;
                newser.SheetTotal = s.Sheets.Count();
                newser.CharSheets = new List<CharSheetMini>();

                foreach (var s2 in s.Sheets) newser.CharSheets.Add(new CharSheetMini(s2, _context));

                newser.Tags = new List<TagOut>();

                if (s.Series.Tags != null)
                {
                    var taglist = JObject.Parse(s.Series.Tags.RootElement.ToString());
                    foreach (var tag in taglist["SeriesTags"])
                        newser.Tags.Add(new TagOut(allTagsList.Where(atl => atl.Guid == (Guid)tag).FirstOrDefault()));
                }

                outputSeries.Add(newser);
            }


            return Ok(outputSeries);
        }

        return Unauthorized();
    }


    /// <summary>
    ///     This lists all series and their titles along with ids, and nothing else.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Series/ShortList
    [HttpGet("ShortList")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Series>>> GetSeriesList(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var none = await _context.Series.Where(s => s.Isactive == true && s.Title == string.Empty)
                .Select(sc => new { sc.Guid, sc.Title, sc.Titlejpn })
                .OrderBy(x => x.Title).FirstOrDefaultAsync();

            var ser = await _context.Series
                .Where(s => s.Isactive == true && s.Title != string.Empty && allowedSeries.Contains(s.Guid))
                .Select(sc => new { sc.Guid, sc.Title, sc.Titlejpn })
                .OrderBy(x => x.Title).ToListAsync();

            ser.Insert(0, none);

            return Ok(ser.OrderBy(x => StringLogic.IgnorePunct(x.Title)));
        }

        return Unauthorized();


        //return await _context.Series.ToListAsync();
    }


    /// <summary>
    ///     This lists all series and their titles along with ids, and nothing else.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Series/ShortList
    [HttpGet("ShortListWithTags")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Series>>> GetSeriesListWithTags(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var ser = await _context.Series.Where(s => s.Isactive == true && s.Title != string.Empty
                                                                          && allowedSeries.Contains(s.Guid)).Select(
                    sc => new
                    {
                        sc.Guid,
                        sc.Title,
                        sc.Titlejpn,
                        sc.Tags
                    }).OrderBy(x => StringLogic.IgnorePunct(x.Title))
                .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                .Take(pagingParameterModel.pageSize).ToListAsync();


            var serOutPut = new List<Seri>();

            foreach (var s in ser)
            {
                var newOutput = new Seri
                {
                    Guid = s.Guid,
                    Title = s.Title,
                    Titlejpn = s.Titlejpn,
                    Tags = new List<TagOut>()
                };

                if (s.Tags != null)
                {
                    var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                    foreach (var tag in taglist)
                    foreach (var tagguid in tag.Value)
                    {
                        var pulledtag = await _context.Tags.Where(s => s.Guid == Guid.Parse(tagguid.ToString()))
                            .Include("Tagtype").FirstOrDefaultAsync();
                        newOutput.Tags.Add(new TagOut(pulledtag));
                    }


                    //newser.Tags
                }

                serOutPut.Add(newOutput);
            }

            var output = new SeriListOut();
            output.SeriList = serOutPut.OrderBy(x => StringLogic.IgnorePunct(x.Title)).ToList();
            output.fulltotal = (allowedSeries.Count + pagingParameterModel.pageSize - 2) /
                               pagingParameterModel.pageSize;

            return Ok(output);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     This lists all series and their titles along with ids, and nothing else.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Series/ShortList
    [HttpGet("FullShortListWithTags")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Series>>> GetFullSeriesListWithTags()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var ser = await _context.Series
                .Where(s => s.Isactive == true && allowedSeries.Contains(s.Guid) && s.Title != "")
                .OrderBy(o => o.Title)
                .Select(s => new
                {
                    Series = s,
                    Sheets = _context.CharacterSheetApproveds.Where(csa => csa.Isactive == true
                                                                           && csa.Seriesguid == s.Guid)
                        .OrderBy(csa => csa.Name).ToList(),
                    TagsList = s.SeriesTags.Select(ist => ist.Tag).OrderBy(cst => cst.Name).ToList()
                })
                .ToListAsync();
            var serOutPut = new List<Seri>();

            foreach (var s in ser)
            {
                var newOutput = new Seri
                {
                    Guid = s.Series.Guid,
                    Title = s.Series.Title,
                    Titlejpn = s.Series.Titlejpn,
                    Tags = new List<TagOut>(),
                    SheetTotal = s.Sheets.Count()
                };

                if (s.TagsList.Count > 0)
                    foreach (var tag in s.TagsList)
                        newOutput.Tags.Add(new TagOut(tag));

                serOutPut.Add(newOutput);
            }

            var output = new SeriListOut();
            output.SeriList = serOutPut.OrderBy(x => StringLogic.IgnorePunct(x.Title)).ToList();
            output.fulltotal = allowedSeries.Count;

            return Ok(output);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     This lists all series and their titles along with ids, and nothing else.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Series/ShortList
    [HttpGet("FullShortListWithTagsAndDeactive")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Series>>> GetFullSeriesListWithTagsAndDeactive()
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var ser = await _context.Series.Where(s => s.Title != "")
              .OrderBy(o => o.Title)
              .Select(s => new
              {
                  Series = s,
                  TagsList = s.SeriesTags.Select(ist => ist.Tag).ToList()
              })
                 .ToListAsync();
            var serOutPut = new List<Seri>();

            foreach (var s in ser)
            {
                var newOutput = new Seri
                {
                    Guid = s.Series.Guid,
                    Title = s.Series.Title,
                    Titlejpn = s.Series.Titlejpn,
                    Tags = new List<TagOut>(),
                    Isactive = s.Series.Isactive,
                    Createdate = s.Series.Createdate,
                    Deactivedate = s.Series.Deactivedate
                };

                if (s.TagsList.Count > 0)
                    foreach (var tag in s.TagsList)
                        newOutput.Tags.Add(new TagOut(tag));

                serOutPut.Add(newOutput);
            }

            var output = new SeriListOut();
            output.SeriList = serOutPut.OrderBy(x => StringLogic.IgnorePunct(x.Title)).ToList();

            return Ok(output);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Finds a series based on a series tag guid.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    // GET: api/v1/Series/list
    [HttpGet("FindByTag/{tagGuid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Series>>> GetSeriesListbyTag(
        [FromQuery] PagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var foundTag = await _context.Tags
                .Where(t => t.Isactive == true && (t.Tagtype.Name == "Series") && t.Guid == pagingParameterModel.guid)
                .FirstOrDefaultAsync();

            if (foundTag == null) return NotFound();


            var ser = await _context.Series.Where(s => s.Isactive == true && allowedSeries.Contains(s.Guid) &&
                                                       s.Tags.RootElement.GetProperty("Tags").GetString()
                                                           .Contains(pagingParameterModel.guid.ToString())).Select(sc =>
                    new { sc.Guid, sc.Title, sc.Titlejpn })
                .OrderBy(x => StringLogic.IgnorePunct(x.Title))
                .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                .Take(pagingParameterModel.pageSize).ToListAsync();

            return Ok(ser);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Search for series based on partial series name.
    /// </summary>
    /// <returns></returns>
    // GET: api/v1/Search
    [HttpGet("Search/")]
    [Authorize]
    public async Task<ActionResult<Series>> GetSeriesSearchPartial(
        [FromQuery] SeriesPagingParameterModel pagingParameterModel)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            var initSeries = await _context.Series.Where(c => c.Isactive == true && allowedSeries.Contains(c.Guid))
                .ToListAsync();

            var taggedSeries = new List<Series>();

            if (pagingParameterModel.tags != null && pagingParameterModel.tags.Length > 0)
            {
                foreach (var ser in initSeries)
                    if (ser.Tags != null)
                    {
                        var taglist = JObject.Parse(ser.Tags.RootElement.ToString())["SeriesTags"].ToList();
                        var tags2 = taglist.Values<string>().Select(s => Guid.Parse(s)).ToList();
                        var alltagsfound = pagingParameterModel.tags.Intersect(tags2).Count();
                        if (alltagsfound == pagingParameterModel.tags.Length) taggedSeries.Add(ser);
                    }
            }
            else
            {
                taggedSeries = initSeries;
            }

            var series = taggedSeries.Where(s => s.Title != string.Empty
                                                 && (pagingParameterModel.titleinput == null || s.Title.ToLower()
                                                     .Contains(pagingParameterModel.titleinput.ToLower()))
                                                 && (pagingParameterModel.jpntitleinput == null || s.Titlejpn.ToLower()
                                                     .Contains(pagingParameterModel.jpntitleinput.ToLower())))
                .OrderBy(x => StringLogic.IgnorePunct(x.Title))
                .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                .Take(pagingParameterModel.pageSize).ToList();

            series = series.Where(ser => ser.Isactive == true).ToList();

            if (series == null) return NotFound();

            var serOutPut = new List<Seri>();

            foreach (var s in series)
            {
                var newOutput = new Seri
                {
                    Guid = s.Guid,
                    Title = s.Title,
                    Titlejpn = s.Titlejpn,
                    Tags = new List<TagOut>()
                };

                if (s.Tags != null)
                {
                    var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                    foreach (var tag in taglist)
                    foreach (var tagguid in tag.Value)
                    {
                        var pulledtag = await _context.Tags.Where(s => s.Guid == Guid.Parse(tagguid.ToString()))
                            .Include("Tagtype").FirstOrDefaultAsync();
                        newOutput.Tags.Add(new TagOut(pulledtag));
                    }


                    //newser.Tags
                }

                serOutPut.Add(newOutput);
            }


            return Ok(serOutPut);
        }

        return Unauthorized();
    }


    // GET: api/v1/Series/5
    [HttpGet("{guid}")]
    [Authorize]
    public async Task<ActionResult<Series>> GetSeries(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            if (!allowedSeries.Contains(guid)) return Unauthorized();

            var series = await _context.Series.Where(s => s.Guid == guid && allowedSeries.Contains(s.Guid))
                .ToListAsync();

            if (series == null) return NotFound();

            var outputSeries = new List<Seri>();

            foreach (var s in series)
            {
                var newser = new Seri();

                newser.Guid = s.Guid;
                newser.Title = s.Title;
                newser.Titlejpn = s.Titlejpn;
                newser.Isactive = s.Isactive;
                newser.Createdate = s.Createdate;
                newser.Deactivedate = s.Deactivedate;
                newser.Tags = new List<TagOut>();

                if (s.Tags != null)
                {
                    var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                    foreach (var tag in taglist)
                    foreach (var tagguid in tag.Value)
                    {
                        var pulledtag = await _context.Tags.Where(s => s.Guid == Guid.Parse(tagguid.ToString()))
                            .Include("Tagtype").FirstOrDefaultAsync();
                        newser.Tags.Add(new TagOut(pulledtag));
                    }
                }

                newser.Createdate = s.Createdate;


                outputSeries.Add(newser);
            }


            return Ok(outputSeries);
        }

        return Unauthorized();
    }

    // GET: api/v1/Series/5/characters
    [HttpGet("{guid}/WithPendingCharacters")]
    [Authorize]
    public async Task<ActionResult<object>> GetSeriesWithChars(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            //var series = await _context.Series.FindAsync(id);

            var allowedSeries = GetAllowedSeries(authId, accessToken);

            if (!allowedSeries.Contains(guid)) return Unauthorized();

            var series = await _context.Series
                .Where(s => s.Isactive == true && s.Guid == guid && allowedSeries.Contains(s.Guid)).Select(sc => new
                {
                    sc.Guid,
                    sc.Title,
                    sc.Titlejpn,
                    Characters = sc.CharacterSheets.Where(ch => ch.Isactive == true)
                        .Select(cha => new { cha.Guid, cha.Name })
                }).ToListAsync();


            if (series == null) return NotFound();


            return Ok(series);
        }

        return Unauthorized();
    }


    // GET: api/v1/Series/5/characters
    [HttpGet("{guid}/WithApprovedCharacters")]
    [Authorize]
    public async Task<ActionResult<object>> GetSeriesWithApprovedCharacters(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            //var series = await _context.Series.FindAsync(id);

            var allowedSeries = GetAllowedSeries(authId, accessToken);

            if (!allowedSeries.Contains(guid)) return Unauthorized();


            var series = await _context.Series.Where(s => s.Isactive == true && s.Guid == guid).Select(sc => new
            {
                sc.Guid,
                sc.Title,
                sc.Titlejpn,
                Characters = sc.CharacterSheetApproveds.Where(ch => ch.Isactive == true)
                    .Select(cha => new { cha.Guid, cha.Name })
            }).ToListAsync();


            if (series == null) return NotFound();


            return Ok(series);
        }

        return Unauthorized();
    }


    // GET: api/v1/Series/5/characters
    [HttpGet("{guid}/WithPendingItems")]
    [Authorize]
    public async Task<ActionResult<object>> GetSeriesWithCharsandSheetItems(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            if (!allowedSeries.Contains(guid)) return Unauthorized();

            //var series = await _context.Series.FindAsync(id);

            var items = await _context.Series.Where(cs => cs.Isactive == true && cs.Guid == guid)
                .Select(csi => new
                {
                    csi.Guid,
                    csi.Title,
                    csi.Titlejpn,
                    Items = csi.ItemSheets.Where(i => i.Isactive == true).Select(ite => new { ite.Guid, ite.Name })
                }).ToListAsync();


            if (items == null) return NotFound();


            return Ok(items);
        }

        return Unauthorized();
    }


    // GET: api/v1/Series/5/characters
    [HttpGet("{guid}/WithApprovedItems")]
    [Authorize]
    public async Task<ActionResult<object>> GetSeriesWithApprovedItems(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
        {
            var allowedSeries = GetAllowedSeries(authId, accessToken);

            if (!allowedSeries.Contains(guid)) return Unauthorized();

            //var series = await _context.Series.FindAsync(id);

            var items = await _context.Series.Where(cs => cs.Isactive == true && cs.Guid == guid)
                .Select(csi => new
                {
                    csi.Guid,
                    csi.Title,
                    csi.Titlejpn,
                    Items = csi.ItemSheetApproveds.Where(i => i.Isactive == true)
                        .Select(ite => new { ite.Guid, ite.Name })
                }).ToListAsync();


            if (items == null) return NotFound();

            return Ok(items);
        }

        return Unauthorized();
    }

    // GET: api/LinkedCharactersAndItems/5
    [HttpGet("LinkedCharactersAndItems/{guid}")]
    [Authorize]
    public async Task<ActionResult<ListItemsAndCharacters>> GetAllCharactersAndItemsLinkedSeries(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Reader", _context))
            try
            {
                var output = new ListItemsAndCharacters();

                var seriesList = await _context.Series.Where(s => s.Isactive == true && s.Guid == guid).ToListAsync();

                if (seriesList.Count == 0) return BadRequest("Item Not Found");

                var itemsListApproved = await _context.ItemSheetApproveds
                    .Where(ish => ish.Isactive == true && ish.Series.Guid == guid).ToListAsync();
                foreach (var item in itemsListApproved) output.ItemLists.AddApproved(item);

                var itemsList = await _context.ItemSheets.Where(ish => ish.Isactive == true && ish.Series.Guid == guid)
                    .ToListAsync();
                foreach (var item in itemsList) output.ItemLists.AddUnapproved(item);

                var characterListApproved = await _context.CharacterSheetApproveds
                    .Where(ish => ish.Isactive == true && ish.Series.Guid == guid).ToListAsync();
                foreach (var character in characterListApproved) output.CharacterLists.AddApproved(character);

                var characterList = await _context.CharacterSheets
                    .Where(ish => ish.Isactive == true && ish.Series.Guid == guid).ToListAsync();
                foreach (var character in characterList) output.CharacterLists.AddUnapproved(character);

                return output;
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        return Unauthorized();
    }

    // PUT: api/Series/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPut("{guid}")]
    [Authorize]
    public async Task<IActionResult> PutSeries(Guid guid, SeriInput series)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var allowedTags = GetAllowedUserTags(authId, accessToken);

            if (guid != series.Guid) return BadRequest();
            var title = await _context.Series.Where(s => s.Guid == guid).FirstOrDefaultAsync();
            var seriesTags = await _context.SeriesTags.Where(st => st.SeriesGuid == guid).ToListAsync();


            if (title.Title != series.Title) title.Title = series.Title;
            if (title.Titlejpn != series.Titlejpn) title.Titlejpn = series.Titlejpn;
            if (title.Isactive != series.Isactive) title.Titlejpn = series.Titlejpn;

            if (series.Tags != null)
            {
                foreach (var tag in series.Tags)
                {
                    if (!allowedTags.Contains(tag))
                        return Unauthorized();

                    if (!seriesTags.Any(st => st.TagGuid == tag))
                    {
                        SeriesTag newTag = new SeriesTag()
                        {
                            SeriesGuid = guid,
                            TagGuid = tag
                        };

                        _context.SeriesTags.Add(newTag);
                    }
                }

                foreach(var sTag in seriesTags)
                {
                    if(!series.Tags.Contains(sTag.TagGuid))
                    {
                        _context.SeriesTags.Remove(sTag);
                    }
                }

                var json = JsonSerializer.Serialize(series.Tags);
                json = @"{""SeriesTags"":" + json + "}";
                title.Tags = JsonDocument.Parse(json);
            }
            else
            {
                title.Tags = null;
            }


            _context.Entry(title).State = EntityState.Modified;

            try
            {
                var results = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeriesExists(guid))
                    return NotFound();
                throw;
            }

            return Ok(series);
        }

        return Unauthorized();
    }

    // POST: api/Series
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Series>> PostSeries([FromBody] SeriInput input)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Writer", _context))
        {
            var allowedTags = GetAllowedUserTags(authId, accessToken);

            var newSeries = new Series()
            {
                Guid=Guid.NewGuid(),
                Title = input.Title ?? null,
                Titlejpn = input.Titlejpn ?? null,
                Isactive=true
            };

            if (input.Tags != null)
            {
                foreach (var tag in input.Tags)
                {
                    if (!allowedTags.Contains(tag))
                        return Unauthorized();

                    SeriesTag newTag = new SeriesTag()
                    {
                        SeriesGuid = newSeries.Guid,
                        TagGuid = tag
                    };

                    _context.SeriesTags.Add(newTag);
                }

                var json = JsonSerializer.Serialize(input.Tags);
                json = @"{""SeriesTags"":" + json + "}";
                newSeries.Tags = JsonDocument.Parse(json);
            }


            _context.Series.Add(newSeries);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSeries", new { id = newSeries.Guid }, newSeries);
        }

        return Unauthorized();
    }


    // POST: api/Series
    // To protect from overposting attacks, enable the specific properties you want to bind to, for
    // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
    [HttpPost("dumpin")]
    public async Task<ActionResult<Series>> PostSeriesDump([FromBody] SeriInput input)
    {
        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);

        if (accessToken != "IAmABanana!")
        {
            return Unauthorized();
        }

        var newSeries = new Series()
        {
            Guid = input.Guid,
            Title = input.Title ?? null,
            Titlejpn = input.Titlejpn ?? null,
        };

        _context.Series.Add(newSeries);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSeries", new { id = newSeries.Guid }, newSeries);
    }

    // DELETE: api/Series/5
    [HttpDelete("{guid}")]
    [Authorize]
    public async Task<ActionResult<Series>> DeleteSeries(Guid guid)
    {
        var authId = HttpContext.User.Claims.ToList()[1].Value;

        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
        // Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);

        // if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
        {
            var allowedTags = GetAllowedUserTags(authId, accessToken);

            var series = await _context.Series.FindAsync(guid);
            if (series == null) return NotFound();

            series.Isactive = false;
            series.Deactivedate = DateTime.Now;

            _context.Series.Update(series);
            await _context.SaveChangesAsync();

            return series;
        }

        return Unauthorized();
    }

    private bool SeriesExists(Guid id)
    {
        return _context.Series.Any(e => e.Guid == id);
    }


    private List<Guid> GetAllowedSeries(string authId, string accessToken)
    {
        var legalsheets = _context.Series.Where(it => it.Isactive == true)
            .Select(it => new TagScanContainer(it.Guid, it.SeriesTags)).ToList();
        var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
            .Select(ulr => (Guid)ulr.Larpguid).ToList();

        var allowedTags = _context.Larptags.Where(lt =>
            (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
            && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        return TagScanner.ScanTagsSeries(legalsheets, allowedTags);
    }

    private List<Guid?> GetAllowedUserTags(string authId, string accessToken)
    {
        var legalsheets = _context.Series.Where(it => it.Isactive == true)
            .Select(it => new TagScanContainer(it.Guid, it.SeriesTags)).ToList();
        var allowedLARPS = _context.UserLarproles.Where(ulr => ulr.User.Authid == authId && ulr.Isactive == true)
            .Select(ulr => (Guid)ulr.Larpguid).ToList();

        var allowedTags = _context.Larptags.Where(lt =>
            (allowedLARPS.Any(al => al == (Guid)lt.Larpguid) || lt.Larpguid == null)
            && lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        if (UsersLogic.IsUserAuthed(authId, accessToken, "Wizard", _context))
            allowedTags = _context.Larptags.Where(lt => lt.Isactive == true).Select(lt => lt.Tagguid).ToList();

        return allowedTags;
    }
}