using Microsoft.EntityFrameworkCore;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEXUSDataLayerScaffold.Logic
{
    public class ReviewMessageLogic
    {
        private NexusLarpLocalContext _context;
        public ReviewMessageLogic(NexusLarpLocalContext context)
        {
            _context = context;
        }

        //Get
        public async Task<ItemSheetReviewMessage> GetItemMessage(int id)
        {
            try
            {
                return _context.ItemSheetReviewMessages
                    .Where(isrm => isrm.Id == id).FirstOrDefault();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<CharacterSheetReviewMessage> GetCharacterMessage(int id)
        {
            try
            {
                return _context.CharacterSheetReviewMessages
                    .Where(isrm => isrm.Id == id).FirstOrDefault();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessage>> GetAllItemMessagesCreatedByUser(Guid userGuid)
        {
            try
            {
                return _context.ItemSheetReviewMessages
                    .Where(isrm => isrm.CreatedbyuserGuid == userGuid)
                    .Select(x => new ReviewMessage(x, _context)).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessage>> GetAllCharacterMessagesCreatedByUser(Guid userGuid)
        {
            try
            {
                var usersList = _context.Users.Select(x => x).ToList();

                return _context.CharacterSheetReviewMessages
                    .Where(isrm => isrm.CreatedbyuserGuid == userGuid)
                    .Select(x => new ReviewMessage(x, usersList)).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public async Task<List<ReviewMessage>> GetAllItemMessagesforItem(Guid isheetGuid)
        {
            try
            {
                List<ReviewMessage> retList = new List<ReviewMessage>();

                List<int> itemIds = await _context.ItemSheets.Where(i => i.Guid == isheetGuid)
                    .Select(i => i.Id).ToListAsync();
                var messages = await _context.ItemSheetReviewMessages
                    .Where(isrm => itemIds.Contains(isrm.ItemsheetId)).ToListAsync();

                foreach(var mess in messages)
                {
                    retList.Add(new ReviewMessage(mess, _context));
                }

                return retList;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessage>> GetAllActiveItemMessagesforItem(Guid isheetGuid)
        {
            try
            {
                return GetAllItemMessagesforItem(isheetGuid).Result.Where(rm => rm.IsActive == true).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public async Task<List<ReviewMessage>> GetAllCharacterMessagesforCharacter(Guid charGuid)
        {
            try
            {
                List<ReviewMessage> retList = new List<ReviewMessage>();

                List<int> charIds = _context.CharacterSheets.Where(i => i.Guid == charGuid)
                    .Select(i => i.Id).ToList();
                var messages = await _context.CharacterSheetReviewMessages
                    .Where(isrm => charIds.Contains(isrm.CharactersheetId)).ToListAsync();

                var usersList = _context.Users.Select(x => x).ToList();
                foreach (var mess in messages)
                {
                    retList.Add(new ReviewMessage(mess, usersList));
                }

                return retList;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessage>> GetAllActiveCharacterMessagesforCharacter(Guid charGuid)
        {
            try
            {
                return GetAllCharacterMessagesforCharacter(charGuid).Result.Where(rm => rm.IsActive == true).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Create
        public async Task<int> AddItemMessage(ReviewMessage rm)
        {
            int returnnum = -1;
            try
            {
                var rmconvert = rm.ConvertToItemSheetMessage();
                _context.ItemSheetReviewMessages.Add(rmconvert);
                await _context.SaveChangesAsync();

                returnnum = rmconvert.Id;
                return returnnum;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> AddCharacterMessage(ReviewMessage rm)
        {
            int returnnum = -1;
            try
            {
                var rmconvert = rm.ConvertToCharacterSheetMessage();
                _context.CharacterSheetReviewMessages.Add(rmconvert);
                await _context.SaveChangesAsync();

                returnnum = rmconvert.Id;
                return returnnum;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Update
        public async Task<bool> UpdateItemMessage(ReviewMessage rm)
        {
            try
            {
                var message = await GetItemMessage(rm.Id);

                if (message == null)
                {
                    return false;
                }

                message.Message = rm.Message;
                message.Createdate = rm.Createdate;
                message.CreatedbyuserGuid = rm.CreatedbyuserGuid;
                message.Isactive = rm.IsActive;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> UpdateCharacterMessage(ReviewMessage rm)
        {
            try
            {
                var message = await GetCharacterMessage(rm.Id);

                if (message == null)
                {
                    return false;
                }

                message.Message = rm.Message;
                message.Createdate = rm.Createdate;
                message.CreatedbyuserGuid = rm.CreatedbyuserGuid;
                message.Isactive = rm.IsActive;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Soft Delete
        public async Task<bool> DeactivateItemMessage(int Id)
        {
            try
            {
                var message = await GetItemMessage(Id);

                if (message == null)
                {
                    return false;
                }

                message.Isactive = false;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> DeactivateCharacterMessage(int Id)
        {
            try
            {
                var message = await GetCharacterMessage(Id);

                if (message == null)
                {
                    return false;
                }

                message.Isactive = false;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Hard Delete
        public async Task<bool> DeleteItemMessage(int Id)
        {
            try
            {
                var message = await GetItemMessage(Id);

                if (message == null)
                {
                    return false;
                }

                _context.ItemSheetReviewMessages.Remove(message);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> DeleteCharacterMessage(int Id)
        {
            try
            {
                var message = await GetCharacterMessage(Id);

                if (message == null)
                {
                    return false;
                }

                _context.CharacterSheetReviewMessages.Remove(message);
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
