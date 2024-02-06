using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Extensions;

public class Item
{
    public static IteSheet CreateItem(ItemSheet iSheet, NexusLarpLocalContext _context)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(iSheet.Fields.RootElement.ToString()));

        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = FeildsWInit;
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<TagOut>();

        var folderName = Path.Combine("images", "items", "UnApproved");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            

        if (iSheet.Img1 != null) {
            if (System.IO.File.Exists(pathToSave + iSheet.Img1)) {
                newiemSheet.imagedata =
                    System.IO.File.ReadAllBytes(pathToSave + iSheet.Img1);
            }
        }

        if (iSheet.CreatedbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == iSheet.CreatedbyuserGuid)
                .FirstOrDefault();

            newiemSheet.createdby = lookupuser.Preferredname;
            if (lookupuser.Lastname != null && lookupuser.Lastname.Length > 0)
            {
                newiemSheet.createdby += " " + lookupuser.Lastname[0];
            }

            if ((lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty) && lookupuser.Firstname != null)
            {
                newiemSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (newiemSheet.FirstapprovalbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newiemSheet.FirstapprovalbyuserGuid)
                .FirstOrDefault();
            newiemSheet.Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.Firstapprovalby += " " + lookupuser.Lastname[0];
            }
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (newiemSheet.SecondapprovalbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newiemSheet.SecondapprovalbyuserGuid)
                .FirstOrDefault();
            newiemSheet.Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.Secondapprovalby += " " + lookupuser.Lastname[0];
            }
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (newiemSheet.EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newiemSheet.EditbyUserGuid)
                .FirstOrDefault();
            newiemSheet.EditbyUser = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.EditbyUser += " " + lookupuser.Lastname[0];
            }
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.EditbyUser = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        return newiemSheet;
    }

    public static IteSheet CreateItem(ItemSheetApproved iSheet, NexusLarpLocalContext _context) {

        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(iSheet.Fields.RootElement.ToString()));

        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = FeildsWInit;
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<TagOut>();

        var folderName = Path.Combine("images", "items", "Approved");
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);


        if (iSheet.Img1 != null) {
            if (System.IO.File.Exists(pathToSave + "\\" + iSheet.Img1)) {
                newiemSheet.imagedata =
                    System.IO.File.ReadAllBytes(pathToSave + "\\" + iSheet.Img1);
            }
        }


        if (iSheet.CreatedbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == iSheet.CreatedbyuserGuid)
                .FirstOrDefault();

            newiemSheet.createdby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.createdby += " " + lookupuser.Lastname[0];
            }

            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (newiemSheet.FirstapprovalbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newiemSheet.FirstapprovalbyuserGuid)
                .FirstOrDefault();
            newiemSheet.Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.Firstapprovalby += " " + lookupuser.Lastname[0];
            }
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (newiemSheet.SecondapprovalbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newiemSheet.SecondapprovalbyuserGuid)
                .FirstOrDefault();
            newiemSheet.Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.Secondapprovalby += " " + lookupuser.Lastname[0];
            }
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        if (newiemSheet.EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newiemSheet.EditbyUserGuid)
                .FirstOrDefault();
            newiemSheet.EditbyUser = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0)
            {
                newiemSheet.EditbyUser += " " + lookupuser.Lastname[0];
            }
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
            {
                newiemSheet.EditbyUser = lookupuser.Firstname + " " + lookupuser.Lastname;
            }
        }

        return newiemSheet;
    }

    public static IteSheet CreateItem(ItemSheetVersion iSheet)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(iSheet.Fields.RootElement.ToString()));

        var newiemSheet = new IteSheet();

        newiemSheet.Name = iSheet.Name;
        newiemSheet.Guid = iSheet.Guid;
        newiemSheet.Seriesguid = iSheet.Seriesguid;
        newiemSheet.Img1 = iSheet.Img1;
        newiemSheet.Gmnotes = iSheet.Gmnotes;
        newiemSheet.Fields = FeildsWInit;
        newiemSheet.Createdate = iSheet.Createdate;
        newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
        newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
        newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
        newiemSheet.Tags = new List<TagOut>();

        return newiemSheet;
    }
}