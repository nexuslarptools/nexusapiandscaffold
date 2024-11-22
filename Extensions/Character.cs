using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Extensions;

public class Character
{
    public static CharSheet CreateCharSheet(CharacterSheet cSheet, NexusLarpLocalContext _context)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(cSheet.Fields.RootElement.ToString()));


        var newCharSheet = new CharSheet();

        newCharSheet.Guid = cSheet.Guid;
        newCharSheet.Seriesguid = cSheet.Seriesguid;

        newCharSheet.Fields = FeildsWInit;
        newCharSheet.Name = cSheet.Name;
        newCharSheet.Img1 = cSheet.Img1;
        newCharSheet.Img2 = cSheet.Img2;

        newCharSheet.Isactive = cSheet.Isactive;
        newCharSheet.Createdate = cSheet.Createdate;
        newCharSheet.CreatedbyUserGuid = cSheet.EditbyUserGuid;
        newCharSheet.FirstapprovalbyUserGuid = cSheet.FirstapprovalbyuserGuid;
        newCharSheet.Firstapprovaldate = cSheet.Firstapprovaldate;
        newCharSheet.EditbyUserGuid = cSheet.EditbyUserGuid;
        newCharSheet.SecondapprovalbyUserGuid = cSheet.SecondapprovalbyuserGuid;
        newCharSheet.Secondapprovaldate = cSheet.Secondapprovaldate;
        newCharSheet.Gmnotes = cSheet.Gmnotes;
        newCharSheet.Reason4edit = cSheet.Reason4edit;
        newCharSheet.Version = cSheet.Version;
        newCharSheet.Tags = new List<TagOut>();
        newCharSheet.Readyforapproval = cSheet.Readyforapproval;

        if (cSheet.CreatedbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == cSheet.CreatedbyuserGuid)
                .FirstOrDefault();

            newCharSheet.createdby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.createdby += " " + lookupuser.Lastname[0];

            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (newCharSheet.FirstapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newCharSheet.FirstapprovalbyUserGuid)
                .FirstOrDefault();
            newCharSheet.Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.Firstapprovalby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (newCharSheet.SecondapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newCharSheet.SecondapprovalbyUserGuid)
                .FirstOrDefault();
            newCharSheet.Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.Secondapprovalby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (newCharSheet.EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newCharSheet.EditbyUserGuid)
                .FirstOrDefault();
            newCharSheet.Editby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.Editby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.Editby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        var ReviewMessages = new List<ReviewMessage>();

        var ListMessages = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Isactive == true
                                                                               && isrm.CharactersheetId == cSheet.Id)
            .ToList();

        foreach (var message in ListMessages) ReviewMessages.Add(new ReviewMessage(message, _context));

        newCharSheet.ReviewMessages = ReviewMessages;

        return newCharSheet;
    }


    public static CharSheet CreateCharSheet(CharacterSheetApproved cSheet, NexusLarpLocalContext _context)
    {
        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(cSheet.Fields.RootElement.ToString()));

        var newCharSheet = new CharSheet();

        newCharSheet.Guid = cSheet.Guid;
        newCharSheet.Seriesguid = cSheet.Seriesguid;
        newCharSheet.Fields = FeildsWInit;
        newCharSheet.Name = cSheet.Name;
        newCharSheet.Img1 = cSheet.Img1;
        newCharSheet.Img2 = cSheet.Img2;


        newCharSheet.Isactive = cSheet.Isactive;
        newCharSheet.Createdate = cSheet.Createdate;
        newCharSheet.CreatedbyUserGuid = cSheet.CreatedbyuserGuid;
        newCharSheet.EditbyUserGuid = cSheet.EditbyUserGuid;
        newCharSheet.FirstapprovalbyUserGuid = cSheet.FirstapprovalbyuserGuid;
        newCharSheet.Firstapprovaldate = cSheet.Firstapprovaldate;
        newCharSheet.SecondapprovalbyUserGuid = cSheet.SecondapprovalbyuserGuid;
        newCharSheet.Secondapprovaldate = cSheet.Secondapprovaldate;
        newCharSheet.Gmnotes = cSheet.Gmnotes;
        newCharSheet.Reason4edit = cSheet.Reason4edit;
        newCharSheet.Version = cSheet.Version;
        newCharSheet.Tags = new List<TagOut>();


        if (cSheet.CreatedbyuserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == cSheet.CreatedbyuserGuid)
                .FirstOrDefault();

            newCharSheet.createdby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.createdby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.createdby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (newCharSheet.FirstapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newCharSheet.FirstapprovalbyUserGuid)
                .FirstOrDefault();
            newCharSheet.Firstapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.Firstapprovalby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.Firstapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (newCharSheet.SecondapprovalbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newCharSheet.SecondapprovalbyUserGuid)
                .FirstOrDefault();
            newCharSheet.Secondapprovalby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.Secondapprovalby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.Secondapprovalby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        if (newCharSheet.EditbyUserGuid != null)
        {
            var lookupuser = _context.Users.Where(u => u.Guid == newCharSheet.EditbyUserGuid)
                .FirstOrDefault();
            newCharSheet.Editby = lookupuser.Preferredname;
            if (lookupuser.Lastname.Length > 0) newCharSheet.Editby += " " + lookupuser.Lastname[0];
            if (lookupuser.Preferredname == null || lookupuser.Preferredname == string.Empty)
                newCharSheet.Editby = lookupuser.Firstname + " " + lookupuser.Lastname;
        }

        var ReviewMessages = new List<ReviewMessage>();

        var ListMessages = _context.CharacterSheetReviewMessages.Where(isrm => isrm.Isactive == true
                                                                               && isrm.CharactersheetId ==
                                                                               cSheet.CharactersheetId).ToList();

        foreach (var message in ListMessages) ReviewMessages.Add(new ReviewMessage(message, _context));

        newCharSheet.ReviewMessages = ReviewMessages;

        return newCharSheet;
    }

    public static CharSheet CreateCharSheet(CharacterSheetVersion cSheet)
    {
        var newCharSheet = new CharSheet();

        var FeildsWInit = FieldsLogic.AddInitative(JObject.Parse(cSheet.Fields.RootElement.ToString()));

        newCharSheet.Guid = cSheet.Guid;
        newCharSheet.Seriesguid = cSheet.Seriesguid;
        newCharSheet.Fields = FeildsWInit;
        newCharSheet.Name = cSheet.Name;
        newCharSheet.Img1 = cSheet.Img1;
        newCharSheet.Img2 = cSheet.Img2;

        newCharSheet.Isactive = cSheet.Isactive;
        newCharSheet.Createdate = cSheet.Createdate;
        newCharSheet.CreatedbyUserGuid = cSheet.CreatedbyuserGuid;
        newCharSheet.FirstapprovalbyUserGuid = cSheet.FirstapprovalbyuserGuid;
        newCharSheet.Firstapprovaldate = cSheet.Firstapprovaldate;
        newCharSheet.SecondapprovalbyUserGuid = cSheet.SecondapprovalbyuserGuid;
        newCharSheet.Secondapprovaldate = cSheet.Secondapprovaldate;
        newCharSheet.Gmnotes = cSheet.Gmnotes;
        newCharSheet.Reason4edit = cSheet.Reason4edit;
        newCharSheet.Version = cSheet.Version;
        newCharSheet.Tags = new List<TagOut>();


        return newCharSheet;
    }
}