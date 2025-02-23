using System;
using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Models;

public partial class User
{
    public Guid Guid { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Preferredname { get; set; }

    public string Email { get; set; }

    public Guid? Pronounsguid { get; set; }

    public string Discordname { get; set; }

    public string Authid { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedCreatedbyusers { get; set; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedEditbyUsers { get; set; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedFirstapprovalbyusers { get; set; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedSecondapprovalbyusers { get; set; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheet> CharacterSheetCreatedbyusers { get; set; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheet> CharacterSheetEditbyUsers { get; set; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheet> CharacterSheetFirstapprovalbyusers { get; set; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheetMessageAck> CharacterSheetMessageAcks { get; set; } = new List<CharacterSheetMessageAck>();

    public virtual ICollection<CharacterSheetReviewMessage> CharacterSheetReviewMessages { get; set; } = new List<CharacterSheetReviewMessage>();

    public virtual ICollection<CharacterSheetReviewSubscription> CharacterSheetReviewSubscriptions { get; set; } = new List<CharacterSheetReviewSubscription>();

    public virtual ICollection<CharacterSheet> CharacterSheetSecondapprovalbyusers { get; set; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionCreatedbyusers { get; set; } = new List<CharacterSheetVersion>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionFirstapprovalbyusers { get; set; } = new List<CharacterSheetVersion>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionSecondapprovalbyusers { get; set; } = new List<CharacterSheetVersion>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedCreatedbyusers { get; set; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedEditbyUsers { get; set; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedFirstapprovalbyusers { get; set; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedSecondapprovalbyusers { get; set; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheet> ItemSheetCreatedbyusers { get; set; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheet> ItemSheetEditbyUsers { get; set; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheet> ItemSheetFirstapprovalbyusers { get; set; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheetMessageAck> ItemSheetMessageAcks { get; set; } = new List<ItemSheetMessageAck>();

    public virtual ICollection<ItemSheetReviewMessage> ItemSheetReviewMessages { get; set; } = new List<ItemSheetReviewMessage>();

    public virtual ICollection<ItemSheetReviewSubscription> ItemSheetReviewSubscriptions { get; set; } = new List<ItemSheetReviewSubscription>();

    public virtual ICollection<ItemSheet> ItemSheetSecondapprovalbyusers { get; set; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersionCreatedbyusers { get; set; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersionFirstapprovalbyusers { get; set; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersionSecondapprovalbyusers { get; set; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemUsersContact> ItemUsersContactCreatedbyusers { get; set; } = new List<ItemUsersContact>();

    public virtual ICollection<ItemUsersContact> ItemUsersContactUsers { get; set; } = new List<ItemUsersContact>();

    public virtual ICollection<LarpplayerCharacterSheetAllowed> LarpplayerCharacterSheetAlloweds { get; set; } = new List<LarpplayerCharacterSheetAllowed>();

    public virtual ICollection<LarpplayerCharacterSheetDisllowed> LarpplayerCharacterSheetDislloweds { get; set; } = new List<LarpplayerCharacterSheetDisllowed>();

    public virtual ICollection<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; set; } = new List<LarpplayerSeriesAllowed>();

    public virtual ICollection<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; set; } = new List<LarpplayerSeriesDisllowed>();

    public virtual ICollection<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; set; } = new List<LarpplayerTagAllowed>();

    public virtual ICollection<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; set; } = new List<LarpplayerTagDisllowed>();

    public virtual ICollection<LarprunPreReg> LarprunPreRegCharactersheetRegisteredApprovedbyUserNavigations { get; set; } = new List<LarprunPreReg>();

    public virtual ICollection<LarprunPreReg> LarprunPreRegUsers { get; set; } = new List<LarprunPreReg>();

    public virtual ICollection<Larprun> Larpruns { get; set; } = new List<Larprun>();

    public virtual Pronoun Pronouns { get; set; }

    public virtual ICollection<SheetUsersContact> SheetUsersContactCreatedbyusers { get; set; } = new List<SheetUsersContact>();

    public virtual ICollection<SheetUsersContact> SheetUsersContactUsers { get; set; } = new List<SheetUsersContact>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual ICollection<UserLarprole> UserLarproles { get; set; } = new List<UserLarprole>();
}
