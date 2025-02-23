using Microsoft.AspNetCore.Http.HttpResults;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Linq;

namespace NEXUSDataLayerScaffold.Entities
{
    public static class NameFinder
    {

        public static string GetUserName(Guid userGuid, NexusLarpLocalContext _context)
        {
            try
            {
                if (userGuid != null)
                {
                    var creUser = _context.Users.Where(u => u.Guid == userGuid)
                        .FirstOrDefault();
                    if (creUser.Preferredname == null || creUser.Preferredname == string.Empty)
                        return creUser.Firstname + " " + creUser.Lastname;
                    return creUser.Preferredname;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string GetCharacterName(Guid cGuid, NexusLarpLocalContext _context)
        {
            try
            {
                if (cGuid != null)
                {
                    var unapp = _context.CharacterSheets.Where(cs => cs.Isactive == true && cs.Guid == cGuid)
                        .FirstOrDefault();
                    if (unapp != null)
                    {
                        return unapp.Name;
                    }

                    var app = _context.CharacterSheetApproveds.Where(cs => cs.Isactive == true && cs.Guid == cGuid)
                       .FirstOrDefault();
                    if (app != null)
                    {
                        return app.Name;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public static string GetItemName(Guid iGuid, NexusLarpLocalContext _context)
        {
            try
            {
                if (iGuid != null)
                {
                    var unapp = _context.ItemSheets.Where(cs => cs.Isactive == true && cs.Guid == iGuid)
                   .FirstOrDefault();
                    if (unapp != null)
                    {
                        return unapp.Name;
                    }

                    var app = _context.ItemSheetApproveds.Where(cs => cs.Isactive == true && cs.Guid == iGuid)
                       .FirstOrDefault();
                    if (app != null)
                    {
                        return app.Name;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}
