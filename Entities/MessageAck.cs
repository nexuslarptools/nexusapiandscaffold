using NEXUSDataLayerScaffold.Models;
using System;

namespace NEXUSDataLayerScaffold.Entities
{
    public class MessageAck
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public bool Isactive { get; set; }
        public DateTime? Seendate { get; set; }
        public MessageAck() { }

        public MessageAck(ItemSheetMessageAck isma, NexusLarpLocalContext _context)
        {
            Id = isma.Id;
            UserGuid = isma.UserGuid;
            UserName = NameFinder.GetUserName(isma.UserGuid, _context);
            MessageId = isma.ItemsheetreviewmessagesId;
            Isactive = isma.Isactive;
            Seendate = isma.Seendate;
        }

        public MessageAck(CharacterSheetMessageAck isma, NexusLarpLocalContext _context)
        {
            Id = isma.Id;
            UserGuid = isma.UserGuid;
            UserName = NameFinder.GetUserName(isma.UserGuid, _context);
            MessageId = isma.CharactersheetreviewmessagesId;
            Isactive = isma.Isactive;
            Seendate = isma.Seendate;
        }

        public ItemSheetMessageAck ConvertoItemSheetMessageAck()
        {
            return new ItemSheetMessageAck()
            {
                Id = Id,
                UserGuid = UserGuid,
                ItemsheetreviewmessagesId = MessageId,
                Isactive = Isactive,
                Seendate = Seendate
            };
        }

        public CharacterSheetMessageAck ConvertoCharacterSheetMessageAck()
        {
            return new CharacterSheetMessageAck()
            {
                Id = Id,
                UserGuid = UserGuid,
                CharactersheetreviewmessagesId = MessageId,
                Isactive = Isactive,
                Seendate = Seendate
            };
        }
    }
}
