using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class CharSheetMini
{
    public CharSheetMini()
    {
        Guid = Guid.Empty;
        Name = null;
        MainTags = new List<TagOut>();
        AbilityTags = new List<TagOut>();
    }

    public CharSheetMini(CharacterSheetApproved csa, NexusLarpLocalContext _context)
    {
        Guid = csa.Guid;
        Name = csa.Name;
        MainTags = new List<TagOut>();
        AbilityTags = new List<TagOut>();

        if (csa.Taglists != null)
        {
            var result = JsonConvert.DeserializeObject<TagsObject>(csa.Taglists);

            foreach (var tag in result.MainTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag).Include("Tagtype").FirstOrDefaultAsync();
                if (fullTag != null) MainTags.Add(new TagOut(fullTag.Result));
            }

            foreach (var tag in result.AbilityTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag).Include("Tagtype").FirstOrDefaultAsync();
                if (fullTag != null) AbilityTags.Add(new TagOut(fullTag.Result));
            }
        }
    }

    public CharSheetMini(CharacterSheet csa, NexusLarpLocalContext _context)
    {
        Guid = csa.Guid;
        Name = csa.Name;
        MainTags = new List<TagOut>();
        AbilityTags = new List<TagOut>();

        if (csa.Taglists != null)
        {
            var result = JsonConvert.DeserializeObject<TagsObject>(csa.Taglists);

            foreach (var tag in result.MainTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag).Include("Tagtype").FirstOrDefaultAsync();
                if (fullTag != null) MainTags.Add(new TagOut(fullTag.Result));
            }

            foreach (var tag in result.AbilityTags)
            {
                var fullTag = _context.Tags
                    .Where(t => t.Isactive == true && t.Guid == tag).Include("Tagtype").FirstOrDefaultAsync();
                if (fullTag != null) AbilityTags.Add(new TagOut(fullTag.Result));
            }
        }
    }

    public Guid Guid { get; set; }
    public string Name { get; set; }
    public List<TagOut> MainTags { get; set; }
    public List<TagOut> AbilityTags { get; set; }
}