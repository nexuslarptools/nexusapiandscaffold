using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Models;
using System.Text.Json;
using NEXUSDataLayerScaffold.Entities;
using Microsoft.AspNetCore.Authorization;
using NEXUSDataLayerScaffold.Logic;
//using System.Web.Http;

namespace NEXUSDataLayerScaffold.Controllers
{

    [Route("api/v1/[controller]")]
    [ApiController]
    public class SeriesController : ControllerBase
    {
        private readonly NexusLARPContextBase _context;

        public SeriesController(NexusLARPContextBase context)
        {
            _context = context;
        }
        /// <summary>
        /// This gives full information for all series which exist in the database, including the null series.
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/Series
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Series>>> GetSeries()
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var ser = await _context.Series.Where(s => s.Isactive == true).ToListAsync();

                List<Seri> outputSeries = new List<Seri>();

                if (ser == null)
                {
                    return NotFound();
                }

                foreach (Series s in ser)
                {
                    Seri newser = new Seri();

                    newser.Guid = s.Guid;
                    newser.Title = s.Title;
                    newser.Titlejpn = s.Titlejpn;

                    if (s.Tags != null)
                    {
                       var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                        foreach (var tag in taglist)
                        {
                            var value = tag.Value;
                        }


                        //newser.Tags
                    }

                    newser.Createdate = s.Createdate;


                    outputSeries.Add(newser);                    
                }


                return Ok(outputSeries);
            }

            return Unauthorized();
        }

        /// <summary>
        /// This lists all series and their titles along with ids, and nothing else.
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/Series/ShortList
        [HttpGet("ShortList")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Series>>> GetSeriesList([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var none = await _context.Series.Where(s => s.Isactive == true && s.Title == string.Empty).Select(sc => new { sc.Guid, sc.Title, sc.Titlejpn })
                    .OrderBy(x => x.Title).FirstOrDefaultAsync();

                var ser = await _context.Series.Where(s => s.Isactive == true && s.Title != string.Empty ).Select(sc => new { sc.Guid, sc.Title, sc.Titlejpn })
                    .OrderBy(x => x.Title).ToListAsync();

                ser.Insert(0, none);

                return Ok(ser);

            }
            return Unauthorized();
            

            //return await _context.Series.ToListAsync();
        }


        /// <summary>
        /// This lists all series and their titles along with ids, and nothing else.
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/Series/ShortList
        [HttpGet("ShortListWithTags")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Series>>> GetSeriesListWithTags([FromQuery]PagingParameterModel pagingParameterModel)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                var ser = await _context.Series.Where(s => s.Isactive == true && s.Title != string.Empty).Select(sc => new { 
                    sc.Guid, 
                    sc.Title, 
                    sc.Titlejpn,
                    sc.Tags}).OrderBy(x => x.Title)
                    .Skip((pagingParameterModel.pageNumber -1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();


                List<Seri> serOutPut = new List<Seri>();

                foreach (var s in ser)
                {
                    var newOutput = new Seri
                    {
                        Guid = s.Guid,
                        Title = s.Title,
                        Titlejpn = s.Titlejpn,
                        Tags = new List<Tags>(),
                    };

                    if (s.Tags != null)
                    {
                        var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                        foreach (var tag in taglist)
                        {
                            foreach (var tagguid in tag.Value)
                            {
                                var pulledtag = await _context.Tags.Where(s => s.Guid == Guid.Parse(tagguid.ToString())).FirstOrDefaultAsync();
                                newOutput.Tags.Add(pulledtag);
                            }
                        }


                        //newser.Tags
                    }

                    serOutPut.Add(newOutput);
     
                }



                return Ok(serOutPut.OrderBy(x => x.Title));

            }

            return Unauthorized();


            //return await _context.Series.ToListAsync();
        }


        /// <summary>
        /// Finds a series based on a series tag guid.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        // GET: api/v1/Series/list
        [HttpGet("FindByTag/{tagGuid}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Series>>> GetSeriesListbyTag([FromQuery]PagingParameterModel pagingParameterModel)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                var foundTag = await _context.Tags.Where(t => t.Isactive == true && t.Tagtypegu.Name == "Series" && t.Guid == pagingParameterModel.guid).FirstOrDefaultAsync();

                if (foundTag == null)
                {
                    return NotFound();
                }


                var ser = await _context.Series.Where(s => s.Isactive == true &&
                s.Tags.RootElement.GetProperty("Tags").GetString().Contains(pagingParameterModel.guid.ToString())).Select(sc => new { sc.Guid, sc.Title, sc.Titlejpn })
                .OrderBy(x => x.Title)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToListAsync();

                return Ok(ser);

            }
            return Unauthorized();
        }

        /// <summary>
        /// Search for series based on partial series name.
        /// </summary>
        /// <returns></returns>
        // GET: api/v1/Search
        [HttpGet("Search/")]
        [Authorize]
        public async Task<ActionResult<Series>> GetSeriesSearchPartial([FromQuery] SeriesPagingParameterModel pagingParameterModel)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                var initSeries = await _context.Series.Where(c => c.Isactive == true).ToListAsync();

                var taggedSeries = new List<Series>();

                if (pagingParameterModel.tags != null && pagingParameterModel.tags.Length > 0)
                {
                    foreach (var ser in initSeries)
                    {
                        if (ser.Tags != null)
                        {
                            var taglist = JObject.Parse(ser.Tags.RootElement.ToString())["SeriesTags"].ToList();
                            var tags2 = taglist.Values<string>().Select(s => Guid.Parse(s)).ToList();
                            var alltagsfound = pagingParameterModel.tags.Intersect(tags2).Count();
                            if (alltagsfound == pagingParameterModel.tags.Length)
                            {
                                taggedSeries.Add(ser);
                            }
                        }
                    }
                }
                else
                {
                    taggedSeries = initSeries;
                }

                var series = taggedSeries.Where(s => s.Title != string.Empty
                && (pagingParameterModel.titleinput == null || s.Title.ToLower().Contains(pagingParameterModel.titleinput.ToLower())) 
                && (pagingParameterModel.jpntitleinput == null || s.Titlejpn.ToLower().Contains(pagingParameterModel.jpntitleinput.ToLower())))
                    .OrderBy(x => x.Title)
                    .Skip((pagingParameterModel.pageNumber - 1) * pagingParameterModel.pageSize)
                    .Take(pagingParameterModel.pageSize).ToList();

                series = series.Where(ser => ser.Isactive == true).ToList();

                if (series == null)
                {
                    return NotFound();
                }

                List<Seri> serOutPut = new List<Seri>();

                foreach (var s in series)
                {
                    var newOutput = new Seri
                    {
                        Guid = s.Guid,
                        Title = s.Title,
                        Titlejpn = s.Titlejpn,
                        Tags = new List<Tags>(),
                    };

                    if (s.Tags != null)
                    {
                        var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                        foreach (var tag in taglist)
                        {
                            foreach (var tagguid in tag.Value)
                            {
                                var pulledtag = await _context.Tags.Where(s => s.Guid == Guid.Parse(tagguid.ToString())).FirstOrDefaultAsync();
                                newOutput.Tags.Add(pulledtag);
                            }
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
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {

                var series = await _context.Series.Where(s => s.Guid==guid ).ToListAsync();

                if (series == null)
                {
                    return NotFound();
                }

                List<Seri> outputSeries = new List<Seri>();

                foreach (Series s in series)
                {
                    Seri newser = new Seri();

                    newser.Guid = s.Guid;
                    newser.Title = s.Title;
                    newser.Titlejpn = s.Titlejpn;
                    newser.Isactive = s.Isactive;
                    newser.Createdate = s.Createdate;
                    newser.Deactivedate = s.Deactivedate;
                    newser.Tags = new List<Tags>();

                    if (s.Tags != null)
                    {
                        var taglist = JObject.Parse(s.Tags.RootElement.ToString());
                        foreach (var tag in taglist)
                        {
                            foreach (var tagguid in tag.Value)
                            {
                                var pulledtag = await _context.Tags.Where(s => s.Guid == Guid.Parse(tagguid.ToString())).FirstOrDefaultAsync();
                                newser.Tags.Add(pulledtag);
                            }
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
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                //var series = await _context.Series.FindAsync(id);

                var series = await _context.Series.Where(s => s.Isactive == true && s.Guid == guid).Select(sc => new
                {
                    sc.Guid,
                    sc.Title,
                    sc.Titlejpn,
                    Characters = sc.CharacterSheet.Where(ch => ch.Isactive == true).Select(cha => new { cha.Guid, cha.Name })
                }).ToListAsync();



                if (series == null)
                {
                    return NotFound();
                }


                return Ok(series);
            }
            return Unauthorized();
        }


        // GET: api/v1/Series/5/characters
        [HttpGet("{guid}/WithApprovedCharacters")]
        [Authorize]
        public async Task<ActionResult<object>> GetSeriesWithApprovedCharacters(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                //var series = await _context.Series.FindAsync(id);

                var series = await _context.Series.Where(s => s.Isactive == true && s.Guid == guid).Select(sc => new
                {
                    sc.Guid,
                    sc.Title,
                    sc.Titlejpn,
                    Characters = sc.CharacterSheetApproved.Where(ch => ch.Isactive == true).Select(cha => new { cha.Guid, cha.Name })
                }).ToListAsync();



                if (series == null)
                {
                    return NotFound();
                }


                return Ok(series);
            }
            return Unauthorized();
        }



        // GET: api/v1/Series/5/characters
        [HttpGet("{guid}/WithPendingItems")]
        [Authorize]
        public async Task<ActionResult<object>> GetSeriesWithCharsandSheetItems(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                //var series = await _context.Series.FindAsync(id);

                var items = await _context.Series.Where(cs => cs.Isactive == true && cs.Guid==guid)
                .Select(csi => new
                {
                   csi.Guid,
                   csi.Title,
                   csi.Titlejpn,
                   Items = csi.ItemSheet.Where(i => i.Isactive==true).Select(ite => new { ite.Guid, ite.Name})
                }).ToListAsync();


                if (items == null)
                {
                    return NotFound();
                }


                return Ok(items);

            }
            return Unauthorized();
        }


        // GET: api/v1/Series/5/characters
        [HttpGet("{guid}/WithApprovedItems")]
        [Authorize]
        public async Task<ActionResult<object>> GetSeriesWithApprovedItems(Guid guid)
        {
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBRead"))
            {
                //var series = await _context.Series.FindAsync(id);

                var items = await _context.Series.Where(cs => cs.Isactive == true && cs.Guid == guid)
                .Select(csi => new
                {
                    csi.Guid,
                    csi.Title,
                    csi.Titlejpn,
                    Items = csi.ItemSheetApproved.Where(i => i.Isactive == true).Select(ite => new { ite.Guid, ite.Name })
                }).ToListAsync();


                if (items == null)
                {
                    return NotFound();
                }


                return Ok(items);

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

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBWrite"))
            {

                if (guid != series.Guid)
                {
                    return BadRequest();
                }
                var title = await _context.Series.Where(s => s.Guid == guid).FirstOrDefaultAsync();


                if (title.Title != series.Title)
                {
                    title.Title = series.Title;
                }
                if (title.Titlejpn != series.Titlejpn)
                {
                    title.Titlejpn = series.Titlejpn;
                }
                if (title.Isactive != series.Isactive)
                {
                    title.Titlejpn = series.Titlejpn;
                }

                if (series.Tags != null)
                {
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
                   var results =  await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeriesExists(guid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
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
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "SheetDBWrite"))
            {
                Series newSeries = new Series();

                if (input.Title != null)
                {
                    newSeries.Title = input.Title;
                }

                if (input.Titlejpn != null)
                {
                    newSeries.Titlejpn = input.Titlejpn;
                }

                if (input.Tags != null)
                {
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

        // DELETE: api/Series/5
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult<Series>> DeleteSeries(Guid guid)
        {

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Remove(0, 7);
            Task<AuthUser> result = UsersLogic.GetUserInfo(accessToken, _context);
            if (UsersController.UserPermissionAuth(result.Result, "Wizard"))
            {

                var series = await _context.Series.FindAsync(guid);
                if (series == null)
                {
                    return NotFound();
                }

                _context.Series.Remove(series);
                await _context.SaveChangesAsync();

                return series;
            }
            return Unauthorized();
        }

        private bool SeriesExists(Guid id)
        {
            return _context.Series.Any(e => e.Guid == id);
        }
    }
}
