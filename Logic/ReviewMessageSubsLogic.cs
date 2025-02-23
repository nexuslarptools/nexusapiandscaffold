using NEXUSDataLayerScaffold.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using NEXUSDataLayerScaffold.Entities;
using Microsoft.EntityFrameworkCore;

namespace NEXUSDataLayerScaffold.Logic
{
    public class ReviewMessageSubsLogic
    {
        private NexusLarpLocalContext _context;
        public ReviewMessageSubsLogic(NexusLarpLocalContext context)
        {
            _context = context;
        }

        //Get
        public async Task<ItemSheetReviewSubscription> GetItemMessageSub(int id)
        {
            try
            {
                return _context.ItemSheetReviewSubscriptions
                    .Where(isrs => isrs.Id == id).FirstOrDefault();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<CharacterSheetReviewSubscription> GetCharacterMessageSub(int id)
        {
            try
            {
                return _context.CharacterSheetReviewSubscriptions
                    .Where(isrs => isrs.Id == id).FirstOrDefault();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessageSubscription>> GetItemMessageSubsByuser(Guid uGuid)
        {
            try
            {
                return _context.ItemSheetReviewSubscriptions
                    .Where(isrs => isrs.UserGuid == uGuid)
                    .Select(x => new ReviewMessageSubscription(x, _context)).ToList();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessageSubscription>> GetCharacterMessageSubsByUser(Guid uGuid)
        {
            try
            {
                List<ReviewMessageSubscription> output = new List<ReviewMessageSubscription>();
                var isrslist = _context.CharacterSheetReviewSubscriptions
                    .Where(isrs => isrs.UserGuid == uGuid).ToList();

                foreach (var isrs in isrslist)
                {
                    output.Add(new ReviewMessageSubscription(isrs, _context));
                }
                return output;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessageSubscription>> GetItemMessageSubsByItem(Guid iGuid)
        {
            try
            {
                List<ReviewMessageSubscription> output = new List<ReviewMessageSubscription>();
                var isrslist =  await _context.ItemSheetReviewSubscriptions
                    .Where(isrs => isrs.ItemsheetGuid == iGuid && isrs.Stopdate == null).ToListAsync();

                foreach (var isrs in isrslist)
                {
                    output.Add(new ReviewMessageSubscription(isrs, _context));
                }
                return output;


            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ReviewMessageSubscription>> GetCharacterMessageSubsByCharacter(Guid cGuid)
        {
            try
            { 
                List<ReviewMessageSubscription> output = new List<ReviewMessageSubscription>();
                var isrslist = await _context.CharacterSheetReviewSubscriptions
                    .Where(isrs => isrs.CharactersheetGuid == cGuid && isrs.Stopdate == null).ToListAsync();

                foreach (var isrs in isrslist)
                {
                    output.Add(new ReviewMessageSubscription(isrs, _context));
                }
                return output;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Create
        public async Task<int> AddItemMessageSub(ReviewMessageSubscription rms)
        {
            int returnnum = -1;
            try
            {
                _context.ItemSheetReviewSubscriptions.Add(rms.ConvertToItemSheetReviewSubscription());
                await _context.SaveChangesAsync();

                returnnum = rms.Id;
                return returnnum;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> AddCharacterMessageSub(ReviewMessageSubscription rms)
        {
            int returnnum = -1;
            try
            {
                _context.CharacterSheetReviewSubscriptions.Add(rms.ConvertToCharacterSheetReviewSubscription());
                await _context.SaveChangesAsync();

                returnnum = rms.Id;
                return returnnum;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        //Update

        public async Task<bool> UpdateItemMessageSub(ReviewMessageSubscription rms)
        {
            try
            {
                var sub = await GetItemMessageSub(rms.Id);

                if (sub == null)
                {
                    return false;
                }

                sub.UserGuid = rms.UserGuid;
                sub.ItemsheetGuid = rms.SheetGuid;
                sub.Stopdate = rms.Stopdate;


                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> UpdateCharacterMessageSub(ReviewMessageSubscription rms)
        {
            try
            {
                var sub = await GetCharacterMessageSub(rms.Id);

                if (sub == null)
                {
                    return false;
                }

                sub.UserGuid = rms.UserGuid;
                sub.CharactersheetGuid = rms.SheetGuid;
                sub.Stopdate = rms.Stopdate;


                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        //SoftDelete
        public async Task<bool> StopItemMessageSub(int Id)
        {
            try
            {
                var sub = await GetItemMessageSub(Id);

                if (sub == null)
                {
                    return false;
                }

                sub.Stopdate = DateTime.Now;


                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> StopCharacterMessageSub(int Id)
        {
            try
            {
                var sub = await GetCharacterMessageSub(Id);

                if (sub == null)
                {
                    return false;
                }

                sub.Stopdate = DateTime.Now;


                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        //Hard Delete
        public async Task<bool> DeleteItemMessageSub(int Id)
        {
            try
            {
                var sub = await GetItemMessageSub(Id);

                if (sub == null)
                {
                    return false;
                }

                _context.ItemSheetReviewSubscriptions.Remove(sub);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<bool> DeleteCharacterMessageSub(int Id)
        {
            try
            {
                var sub = await GetCharacterMessageSub(Id);

                if (sub == null)
                {
                    return false;
                }

                _context.CharacterSheetReviewSubscriptions.Remove(sub);
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
