using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Helpers
{
    public class SeriesHelper
    {
        public SeriesHelper() { }

        public async Task<List<Seri>> GetSeries(NexusLarpLocalContext _context, string authId, string accessToken)
        {
            var allowedSeries = GetAllowedSeries(_context, authId, accessToken);

            var ser = await _context.Series.Where(s => s.Isactive == true && allowedSeries.Contains(s.Guid) && s.Title != "")
                .Select(s => new
                {
                    Series = s,
                    Sheets = _context.CharacterSheetApproveds.Where(csa => csa.Isactive == true
                    && csa.Seriesguid == s.Guid).ToList()
                })
                .ToListAsync();
            var outputSeries = new List<Seri>();

            if (ser == null) return null;

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
                    {
                        newser.Tags.Add(new TagOut(allTagsList.Where(atl => atl.Guid == (Guid)tag).FirstOrDefault()));
                    }
                }
                outputSeries.Add(newser);
            }

            return outputSeries.OrderBy(o => StringLogic.IgnorePunct(o.Title)).ToList();
        }

        private List<Guid> GetAllowedSeries(NexusLarpLocalContext _context, string authId, string accessToken)
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

    }
}
