using System;
using System.Linq;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Entities;

public class ReviewMessage
{
    public ReviewMessage()
    {
    }

    public ReviewMessage(CharacterSheetReviewMessage reviewMessage, NexusLarpLocalContext _context)
    {
        Id = reviewMessage.Id;
        Message = reviewMessage.Message;
        Createdate = reviewMessage.Createdate;
        CreatedbyuserGuid = reviewMessage.CreatedbyuserGuid;
        IsActive = (bool)reviewMessage.Isactive;

        if (CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == CreatedbyuserGuid)
                .FirstOrDefault();
            createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) createdby = creUser.Firstname;
        }
    }

    public ReviewMessage(ItemSheetReviewMessage reviewMessage, NexusLarpLocalContext _context)
    {
        Id = reviewMessage.Id;
        Message = reviewMessage.Message;
        Createdate = reviewMessage.Createdate;
        CreatedbyuserGuid = reviewMessage.CreatedbyuserGuid;
        IsActive = (bool)reviewMessage.Isactive;

        if (CreatedbyuserGuid != null)
        {
            var creUser = _context.Users.Where(u => u.Guid == CreatedbyuserGuid)
                .FirstOrDefault();
            createdby = creUser.Preferredname;
            if (creUser.Preferredname == null || creUser.Preferredname == string.Empty) createdby = creUser.Firstname;
        }
    }

    public int Id { get; set; }
    public string Message { get; set; }
    public DateTime Createdate { get; set; }
    public Guid? CreatedbyuserGuid { get; set; }
    public string createdby { get; set; }
    public bool IsActive { get; set; }

    public CharacterSheetReviewMessage ConvertToCharacterSheetMessage()
    {
        var Output = new CharacterSheetReviewMessage
        {
            Id = Id,
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
            Message = Message,
            Createdate = Createdate,
            CreatedbyuserGuid = CreatedbyuserGuid,
            Isactive = IsActive
        };

        return Output;
    }
}