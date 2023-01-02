using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities
{
    public class TagsOutput
    {
        public string TagType { get; set; }
        public List<outTag> TagsList { get; set; }

        public TagsOutput()
        {
            TagType = String.Empty;
            TagsList = new List<outTag>();
        }

    }

    public class outTag
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public Guid Tagtypeguid { get; set; }
        public bool? Isactive { get; set; }
        public bool IsLocked { get; set; }
        public List<LARPOut> LarpsTagLockedTo { get; set; }

        public outTag(string name, Guid guid, Guid typeGuid, bool? isactive, bool isLocked)
        {
            this.Name = name;
            this.Guid = guid;
            this.Tagtypeguid = typeGuid;
            this.Isactive = isactive;
            this.IsLocked = IsLocked;
        }
    }
}
