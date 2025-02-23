using NEXUSDataLayerScaffold.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using NEXUSDataLayerScaffold.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NEXUSDataLayerScaffold.Logic
{
    public class ReviewMessageAcksLogic
    {
        private NexusLarpLocalContext _context;

        public ReviewMessageAcksLogic(NexusLarpLocalContext context)
        {
            _context = context;
        }

        //Get
        public async Task<ItemSheetMessageAck> GetItemMessageAck(int id)
        {
            try
            {
                return _context.ItemSheetMessageAcks
                    .Where(isrm => isrm.Id == id).FirstOrDefault();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<CharacterSheetMessageAck> GetCharacterMessageAck(int id)
        {
            try
            {
                return _context.CharacterSheetMessageAcks
                    .Where(isrm => isrm.Id == id).FirstOrDefault();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MessageAck>> GetAllItemMessageAcksForMessage(int id)
        {
            try
            {
                return _context.ItemSheetMessageAcks
                    .Where(isrm => isrm.ItemsheetreviewmessagesId == id)
                    .Select(x => new MessageAck(x, _context)).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MessageAck>> GetAllCharacterMessageAcksForMessage(int id)
        {
            try
            {
                return _context.CharacterSheetMessageAcks
                    .Where(isrm => isrm.CharactersheetreviewmessagesId == id)
                    .Select(x => new MessageAck(x, _context)).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MessageAck>> GetAllItemMessageAcksForMessageByUser(int id, Guid uGuid)
        {
            try
            {
                var output = new List<MessageAck>();
                 var lmaList=  await _context.ItemSheetMessageAcks
                    .Where(isrm => isrm.ItemsheetreviewmessagesId == id && isrm.UserGuid == uGuid).ToListAsync();

                foreach (var lma in lmaList)
                {
                    output.Add( new MessageAck(lma, _context));
                }

                return output;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MessageAck>> GetAllCharacterMessageAcksForMessageByUser(int id, Guid uGuid)
        {
            try
            {

                var output = new List<MessageAck>();
                var lmaList = await _context.CharacterSheetMessageAcks
                    .Where(isrm => isrm.CharactersheetreviewmessagesId == id && isrm.UserGuid == uGuid).ToListAsync();

                foreach (var lma in lmaList)
                {
                    output.Add(new MessageAck(lma, _context));
                }

                return output;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MessageAck>> GetAllUnseenItemMessageAcksForUser(Guid uGuid)
        {
            try
            {
                var output = new List<MessageAck>();
                var mak = _context.ItemSheetMessageAcks
                    .Where(isrm => isrm.Seendate == null && isrm.UserGuid == uGuid).ToList();

                foreach (var ak in mak) 
                {
                    output.Add(new MessageAck(ak, _context));
                }
                return output;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MessageAck>> GetAllUnseenCharacterMessageAcksForUser(Guid uGuid)
        {
            try
            {
                var output = new List<MessageAck>();
                var mak = _context.CharacterSheetMessageAcks
                    .Where(isrm => isrm.Seendate == null && isrm.UserGuid == uGuid).ToList();

                foreach (var ak in mak)
                {
                    output.Add(new MessageAck(ak, _context));
                }
                return output;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Create
        public async Task<int> AddItemMessageAck(MessageAck ma)
        {
            int returnnum = -1;
            try
            {
                _context.ItemSheetMessageAcks.Add(ma.ConvertoItemSheetMessageAck());
                await _context.SaveChangesAsync();

                returnnum = ma.Id;
                return returnnum;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> AddCharacterMessageAck(MessageAck ma)
        {
            int returnnum = -1;
            try
            {
                _context.CharacterSheetMessageAcks.Add(ma.ConvertoCharacterSheetMessageAck());
                await _context.SaveChangesAsync();

                returnnum = ma.Id;
                return returnnum;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Update
        public async Task<bool> UpdateItemMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetItemMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                ack.UserGuid = ma.UserGuid;
                ack.ItemsheetreviewmessagesId = ma.MessageId;
                ack.Isactive = ack.Isactive;
                ack.Seendate = ma.Seendate;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> UpdateCharacterMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetCharacterMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                ack.UserGuid = ma.UserGuid;
                ack.CharactersheetreviewmessagesId = ma.MessageId;
                ack.Isactive = ack.Isactive;
                ack.Seendate = ma.Seendate;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> MarkSeenItemMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetItemMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                ack.Seendate = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> MarkSeenCharacterMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetCharacterMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                ack.Seendate = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Softremove
        public async Task<bool> DeactivateItemMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetItemMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                ack.Isactive = false;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> DeactivateCharacterMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetCharacterMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                ack.Isactive = false;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //HardRemove
        public async Task<bool> DeleteItemMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetItemMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                _context.ItemSheetMessageAcks.Remove(ack);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> DeleteCharacterMessageAck(MessageAck ma)
        {
            try
            {
                var ack = await GetCharacterMessageAck(ma.Id);

                if (ack == null)
                {
                    return false;
                }

                _context.CharacterSheetMessageAcks.Remove(ack);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
