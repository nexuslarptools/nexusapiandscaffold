using Newtonsoft.Json.Linq;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace NEXUSDataLayerScaffold.Extensions
{
    public class Item
    {
        public static IteSheet CreateItem(ItemSheet iSheet)
        {
            IteSheet newiemSheet = new IteSheet();

            newiemSheet.Name = iSheet.Name;
            newiemSheet.Guid = iSheet.Guid;
            newiemSheet.Seriesguid = iSheet.Seriesguid;
            newiemSheet.Img1 = iSheet.Img1;
            newiemSheet.Gmnotes = iSheet.Gmnotes;
            newiemSheet.Fields = JObject.Parse(iSheet.Fields.RootElement.ToString());
            newiemSheet.Createdate = iSheet.Createdate;
            newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
            newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
            newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
            newiemSheet.Tags = new List<Tags>();

            return newiemSheet;

        }

        public static IteSheet CreateItem(ItemSheetApproved iSheet)
        {
            IteSheet newiemSheet = new IteSheet();

            newiemSheet.Name = iSheet.Name;
            newiemSheet.Guid = iSheet.Guid;
            newiemSheet.Seriesguid = iSheet.Seriesguid;
            newiemSheet.Img1 = iSheet.Img1;
            newiemSheet.Gmnotes = iSheet.Gmnotes;
            newiemSheet.Fields = JObject.Parse(iSheet.Fields.RootElement.ToString());
            newiemSheet.Createdate = iSheet.Createdate;
            newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
            newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
            newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
            newiemSheet.Tags = new List<Tags>();


            return newiemSheet;

        }

        public static IteSheet CreateItem(ItemSheetVersion iSheet)
        {
            IteSheet newiemSheet = new IteSheet();

            newiemSheet.Name = iSheet.Name;
            newiemSheet.Guid = iSheet.Guid;
            newiemSheet.Seriesguid = iSheet.Seriesguid;
            newiemSheet.Img1 = iSheet.Img1;
            newiemSheet.Gmnotes = iSheet.Gmnotes;
            newiemSheet.Fields = JObject.Parse(iSheet.Fields.RootElement.ToString());
            newiemSheet.Createdate = iSheet.Createdate;
            newiemSheet.CreatedbyuserGuid = iSheet.CreatedbyuserGuid;
            newiemSheet.FirstapprovalbyuserGuid = iSheet.FirstapprovalbyuserGuid;
            newiemSheet.SecondapprovalbyuserGuid = iSheet.SecondapprovalbyuserGuid;
            newiemSheet.Tags = new List<Tags>();

            return newiemSheet;

        }
    }
}
