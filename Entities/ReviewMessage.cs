using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Linq;

namespace NEXUSDataLayerScaffold.Entities
{
    public class ReviewMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Createdate { get; set; }
        public Guid? CreatedbyuserGuid { get; set; }
        public string createdby { get; set; }
        public bool IsActive { get; set; }

        public ReviewMessage() { }
        public ReviewMessage(CharacterSheetReviewMessage reviewMessage, NexusLarpLocalContext _context) 
        {
            this.Id = reviewMessage.Id;
            this.Message = reviewMessage.Message;
            this.Createdate = reviewMessage.Createdate;
            this.CreatedbyuserGuid = reviewMessage.CreatedbyuserGuid;
            this.IsActive = (bool)reviewMessage.Isactive;

            if (this.CreatedbyuserGuid != null)
            {
                var creUser = _context.Users.Where(u => u.Guid == this.CreatedbyuserGuid)
                    .FirstOrDefault();
                this.createdby = creUser.Preferredname;
                if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                {
                    this.createdby = creUser.Firstname;
                }
            }
        }

        public ReviewMessage(ItemSheetReviewMessage reviewMessage, NexusLarpLocalContext _context)
        {
            this.Id = reviewMessage.Id;
            this.Message = reviewMessage.Message;
            this.Createdate = reviewMessage.Createdate;
            this.CreatedbyuserGuid = reviewMessage.CreatedbyuserGuid;
            this.IsActive = (bool)reviewMessage.Isactive;

            if (this.CreatedbyuserGuid != null)
            {
                var creUser = _context.Users.Where(u => u.Guid == this.CreatedbyuserGuid)
                    .FirstOrDefault();
                this.createdby = creUser.Preferredname;
                if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                {
                    this.createdby = creUser.Firstname;
                }
            }
        }

        public CharacterSheetReviewMessage ConvertToCharacterSheetMessage()
        {
            var Output = new CharacterSheetReviewMessage()
            {
                Id =this.Id,
                Message = this.Message,
                Createdate = this.Createdate,
                CreatedbyuserGuid = this.CreatedbyuserGuid,
                Isactive = this.IsActive,
            };

            return Output;
        }

        public ItemSheetReviewMessage ConvertToItemSheetMessage()
        {
            var Output = new ItemSheetReviewMessage()
            {
                Id = this.Id,
                Message = this.Message,
                Createdate = this.Createdate,
                CreatedbyuserGuid = this.CreatedbyuserGuid,
                Isactive = this.IsActive,
            };

            return Output;
        }
    }
}
