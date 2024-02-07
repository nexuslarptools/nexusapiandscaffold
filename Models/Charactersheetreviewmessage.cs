﻿using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class CharacterSheetReviewMessage
{
    public int Id { get; set; }

    public int CharactersheetId { get; set; }

    public bool? Isactive { get; set; }

    public string Message { get; set; }

    public DateTime Createdate { get; set; }

    public Guid? CreatedbyuserGuid { get; set; }

    public virtual User Createdbyuser { get; set; }
}