﻿using System;

namespace NEXUSDataLayerScaffold.Models;

public class CharacterSheetReviewMessage
{
    public int Id { get; set; }

    public int CharactersheetId { get; set; }

    public bool? Isactive { get; set; }

    public string Message { get; set; }

    public DateTime Createdate { get; set; }

    public Guid? CreatedbyuserGuid { get; set; }

    public virtual User Createdbyuser { get; set; }
}