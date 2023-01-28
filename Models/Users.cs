using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models
{
    public partial class Users
    {
        public Users()
        {
            CharacterSheetApprovedCreatedbyuserGu = new HashSet<CharacterSheetApproved>();
            CharacterSheetApprovedFirstapprovalbyuserGu = new HashSet<CharacterSheetApproved>();
            CharacterSheetApprovedSecondapprovalbyuserGu = new HashSet<CharacterSheetApproved>();
            CharacterSheetCreatedbyuserGu = new HashSet<CharacterSheet>();
            CharacterSheetFirstapprovalbyuserGu = new HashSet<CharacterSheet>();
            CharacterSheetSecondapprovalbyuserGu = new HashSet<CharacterSheet>();
            CharacterSheetVersionCreatedbyuserGu = new HashSet<CharacterSheetVersion>();
            CharacterSheetVersionFirstapprovalbyuserGu = new HashSet<CharacterSheetVersion>();
            CharacterSheetVersionSecondapprovalbyuserGu = new HashSet<CharacterSheetVersion>();
            ItemSheetApprovedCreatedbyuserGu = new HashSet<ItemSheetApproved>();
            ItemSheetApprovedFirstapprovalbyuserGu = new HashSet<ItemSheetApproved>();
            ItemSheetApprovedSecondapprovalbyuserGu = new HashSet<ItemSheetApproved>();
            ItemSheetCreatedbyuserGu = new HashSet<ItemSheet>();
            ItemSheetFirstapprovalbyuserGu = new HashSet<ItemSheet>();
            ItemSheetSecondapprovalbyuserGu = new HashSet<ItemSheet>();
            ItemSheetVersionCreatedbyuserGu = new HashSet<ItemSheetVersion>();
            ItemSheetVersionFirstapprovalbyuserGu = new HashSet<ItemSheetVersion>();
            ItemSheetVersionSecondapprovalbyuserGu = new HashSet<ItemSheetVersion>();
            UserLarproles = new HashSet<UserLarproles>();
        }

        public Guid Guid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Preferredname { get; set; }
        public string Email { get; set; }
        public Guid? Pronounsguid { get; set; }
        public string Discordname { get; set; }
        public string Authid { get; set; }
        public bool? Isactive { get; set; }

        public virtual Pronouns Pronounsgu { get; set; }
        public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedCreatedbyuserGu { get; set; }
        public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedFirstapprovalbyuserGu { get; set; }
        public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedSecondapprovalbyuserGu { get; set; }
        public virtual ICollection<CharacterSheet> CharacterSheetCreatedbyuserGu { get; set; }
        public virtual ICollection<CharacterSheet> CharacterSheetFirstapprovalbyuserGu { get; set; }
        public virtual ICollection<CharacterSheet> CharacterSheetSecondapprovalbyuserGu { get; set; }
        public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionCreatedbyuserGu { get; set; }
        public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionFirstapprovalbyuserGu { get; set; }
        public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionSecondapprovalbyuserGu { get; set; }
        public virtual ICollection<ItemSheetApproved> ItemSheetApprovedCreatedbyuserGu { get; set; }
        public virtual ICollection<ItemSheetApproved> ItemSheetApprovedFirstapprovalbyuserGu { get; set; }
        public virtual ICollection<ItemSheetApproved> ItemSheetApprovedSecondapprovalbyuserGu { get; set; }
        public virtual ICollection<ItemSheet> ItemSheetCreatedbyuserGu { get; set; }
        public virtual ICollection<ItemSheet> ItemSheetFirstapprovalbyuserGu { get; set; }
        public virtual ICollection<ItemSheet> ItemSheetSecondapprovalbyuserGu { get; set; }
        public virtual ICollection<ItemSheetVersion> ItemSheetVersionCreatedbyuserGu { get; set; }
        public virtual ICollection<ItemSheetVersion> ItemSheetVersionFirstapprovalbyuserGu { get; set; }
        public virtual ICollection<ItemSheetVersion> ItemSheetVersionSecondapprovalbyuserGu { get; set; }
        public virtual ICollection<UserLarproles> UserLarproles { get; set; }
    }
}
