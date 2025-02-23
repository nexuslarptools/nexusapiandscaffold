using NEXUSDataLayerScaffold.Models;
using System;

namespace NEXUSDataLayerScaffold.Entities
{
    public class ReviewMessageSubscription
    {
        public ReviewMessageSubscription() 
        { 
        }

        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public Guid SheetGuid { get; set; }
        public string SheetName { get; set; }
        public DateTime Createdate { get; set; }
        public DateTime? Stopdate { get; set; }

        public ReviewMessageSubscription(ItemSheetReviewSubscription sub, NexusLarpLocalContext _context)
        {
            UserName = NameFinder.GetUserName(sub.UserGuid, _context);
            Id = sub.Id;
            UserGuid = sub.UserGuid;
            SheetGuid = sub.ItemsheetGuid;
            SheetName = NameFinder.GetItemName(sub.ItemsheetGuid, _context);
            Createdate = sub.Createdate;
            Stopdate = sub.Stopdate;
        }

        public ReviewMessageSubscription(CharacterSheetReviewSubscription sub, NexusLarpLocalContext _context)
        {
            UserName = NameFinder.GetUserName(sub.UserGuid, _context);
            Id = sub.Id;
            UserGuid = sub.UserGuid;
            SheetGuid = sub.CharactersheetGuid;
            SheetName = NameFinder.GetCharacterName(sub.CharactersheetGuid, _context);
            //Createdate = sub.Createdate;
            Stopdate = sub.Stopdate;
        }

        public ItemSheetReviewSubscription ConvertToItemSheetReviewSubscription()
        {
            var Output = new ItemSheetReviewSubscription
            {
                Id = Id,
                ItemsheetGuid = SheetGuid,
                UserGuid = UserGuid,
                Stopdate = Stopdate,
            };

            return Output;
        }

        public CharacterSheetReviewSubscription ConvertToCharacterSheetReviewSubscription()
        {
            var Output = new CharacterSheetReviewSubscription
            {
                Id = Id,
                CharactersheetGuid = SheetGuid,
                UserGuid = UserGuid,
                Stopdate = Stopdate,
            };

            return Output;
        }
    }
}
