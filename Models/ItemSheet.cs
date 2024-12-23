﻿using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NEXUSDataLayerScaffold.Models;

public class ItemSheet
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public Guid? Seriesguid { get; set; }

    public string Name { get; set; }

    public string Img1 { get; set; }

    public JsonDocument Fields { get; set; }

    public bool? Isactive { get; set; }

    public DateTime Createdate { get; set; }

    public Guid? CreatedbyuserGuid { get; set; }

    public Guid? FirstapprovalbyuserGuid { get; set; }

    public DateTime? Firstapprovaldate { get; set; }

    public Guid? SecondapprovalbyuserGuid { get; set; }

    public DateTime? Secondapprovaldate { get; set; }

    public string Gmnotes { get; set; }

    public string Reason4edit { get; set; }

    public int? Version { get; set; }

    public Guid? EditbyUserGuid { get; set; }

    public string Taglists { get; set; }

    public bool Readyforapproval { get; set; }

    public bool? Isdoubleside { get; set; }

    public JsonDocument Fields2ndside { get; set; }

    public Guid? ItemtypeGuid { get; set; }

    public virtual User Createdbyuser { get; set; }

    public virtual User EditbyUser { get; set; }

    public virtual User Firstapprovalbyuser { get; set; }

    public virtual ICollection<ItemSheetTag> ItemSheetTags { get; } = new List<ItemSheetTag>();

    public virtual ItemType Itemtype { get; set; }

    public virtual User Secondapprovalbyuser { get; set; }

    public virtual Series Series { get; set; }
}