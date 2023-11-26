using System.Collections.Generic;
using System;

namespace NEXUSDataLayerScaffold.Models
{
    public class ListItems
    {
        public List<ShortItemSheetInfo> ApprovedItems { get; set; }
        public List<ShortItemSheetInfo> UnapprovedItems { get; set; }

        public ListItems()
        {
            ApprovedItems = new List<ShortItemSheetInfo>();
            UnapprovedItems = new List<ShortItemSheetInfo>();
        }

        public bool ApprovedContainsGuid(Guid guid)
        {
            foreach(var item in this.ApprovedItems)
            {
                if (item.Guid == guid)
                {
                    return true;
                }
            }
            return false;
        }

        public bool UnapprovedContainsGuid(Guid guid)
        {
            foreach (var item in this.UnapprovedItems)
            {
                if (item.Guid == guid)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddUnapproved(ItemSheet iSheet)
        {
            ShortItemSheetInfo newinfo = new ShortItemSheetInfo();
            newinfo.Guid = iSheet.Guid;
            newinfo.Name = iSheet.Name;
            UnapprovedItems.Add(newinfo);
        }
        public void AddApproved(ItemSheetApproved iSheet)
        {
            ShortItemSheetInfo newinfo = new ShortItemSheetInfo();
            newinfo.Guid = iSheet.Guid;
            newinfo.Name = iSheet.Name;
            ApprovedItems.Add(newinfo);
        }
    }

    public class ShortItemSheetInfo
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
    }
}
