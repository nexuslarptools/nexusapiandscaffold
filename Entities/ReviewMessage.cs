using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class ReviewMessage
{
    public ReviewMessage()
    {
    }

    public ReviewMessage(CharacterSheetReviewMessage reviewMessage, List<User> userList)
    {
        Id = reviewMessage.Id;
        SheetId = reviewMessage.CharactersheetId;
        Message = reviewMessage.Message;
        Createdate = reviewMessage.Createdate;
        CreatedbyuserGuid = reviewMessage.CreatedbyuserGuid;
        IsActive = (bool)reviewMessage.Isactive;
        IsEditable = false;

        if (CreatedbyuserGuid != null)
        {
            var creUser = userList.Where(u => u.Guid == CreatedbyuserGuid)
                .FirstOrDefault();
            createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) createdby = creUser.Firstname + " " + creUser.Lastname;
        }
        Acks = new List<MessageAck>();
    }

    public ReviewMessage(ItemSheetReviewMessage reviewMessage, NexusLarpLocalContext _context)
    {
        Id = reviewMessage.Id;
        SheetId = reviewMessage.ItemsheetId;
        Message = reviewMessage.Message;
        Createdate = reviewMessage.Createdate;
        CreatedbyuserGuid = reviewMessage.CreatedbyuserGuid;
        IsActive = (bool)reviewMessage.Isactive;
        IsEditable = false;

        if (CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == CreatedbyuserGuid)
                .FirstOrDefault();
            createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) createdby = creUser.Firstname + " " + creUser.Lastname;
        }
        Acks = new List<MessageAck>();
    }

    public int Id { get; set; }
    public string Message { get; set; }
    public DateTime Createdate { get; set; }
    public Guid? CreatedbyuserGuid { get; set; }
    public string createdby { get; set; }
    public bool IsActive { get; set; }
    public bool IsEditable { get; set; }
    public int SheetId { get; set; }
    public List<MessageAck> Acks { get; set; }

    public CharacterSheetReviewMessage ConvertToCharacterSheetMessage()
    {
        var Output = new CharacterSheetReviewMessage
        {
            Id = Id,
            CharactersheetId = SheetId,
            Message = Message,
            Createdate = Createdate,
            CreatedbyuserGuid = CreatedbyuserGuid,
            Isactive = IsActive
        };
        return Output;
    }

    public ItemSheetReviewMessage ConvertToItemSheetMessage()
    {
        var Output = new ItemSheetReviewMessage
        {
            Id = Id,
            ItemsheetId = SheetId,
            Message = Message,
            Createdate = Createdate,
            CreatedbyuserGuid = CreatedbyuserGuid,
            Isactive = IsActive
        };
        return Output;
    }
}