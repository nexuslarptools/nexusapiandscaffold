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

    public bool? Isactive { get; set; }

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedCreatedbyusers { get; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedEditbyUsers { get; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedFirstapprovalbyusers { get; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheetApproved> CharacterSheetApprovedSecondapprovalbyusers { get; } = new List<CharacterSheetApproved>();

    public virtual ICollection<CharacterSheet> CharacterSheetCreatedbyusers { get; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheet> CharacterSheetEditbyUsers { get; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheet> CharacterSheetFirstapprovalbyusers { get; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheet> CharacterSheetSecondapprovalbyusers { get; } = new List<CharacterSheet>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionCreatedbyusers { get; } = new List<CharacterSheetVersion>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionFirstapprovalbyusers { get; } = new List<CharacterSheetVersion>();

    public virtual ICollection<CharacterSheetVersion> CharacterSheetVersionSecondapprovalbyusers { get; } = new List<CharacterSheetVersion>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedCreatedbyusers { get; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedEditbyUsers { get; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedFirstapprovalbyusers { get; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheetApproved> ItemSheetApprovedSecondapprovalbyusers { get; } = new List<ItemSheetApproved>();

    public virtual ICollection<ItemSheet> ItemSheetCreatedbyusers { get; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheet> ItemSheetEditbyUsers { get; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheet> ItemSheetFirstapprovalbyusers { get; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheet> ItemSheetSecondapprovalbyusers { get; } = new List<ItemSheet>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersionCreatedbyusers { get; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersionFirstapprovalbyusers { get; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemSheetVersion> ItemSheetVersionSecondapprovalbyusers { get; } = new List<ItemSheetVersion>();

    public virtual ICollection<ItemUsersContact> ItemUsersContactCreatedbyusers { get; } = new List<ItemUsersContact>();

    public virtual ICollection<ItemUsersContact> ItemUsersContactUsers { get; } = new List<ItemUsersContact>();

    public virtual ICollection<LarpplayerCharacterSheetAllowed> LarpplayerCharacterSheetAlloweds { get; } = new List<LarpplayerCharacterSheetAllowed>();

    public virtual ICollection<LarpplayerCharacterSheetDisllowed> LarpplayerCharacterSheetDislloweds { get; } = new List<LarpplayerCharacterSheetDisllowed>();

    public virtual ICollection<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; } = new List<LarpplayerSeriesAllowed>();

    public virtual ICollection<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; } = new List<LarpplayerSeriesDisllowed>();

    public virtual ICollection<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; } = new List<LarpplayerTagAllowed>();

    public virtual ICollection<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; } = new List<LarpplayerTagDisllowed>();

    public virtual ICollection<LarprunPreReg> LarprunPreRegCharactersheetRegisteredApprovedbyUserNavigations { get; } = new List<LarprunPreReg>();

    public virtual ICollection<LarprunPreReg> LarprunPreRegUsers { get; } = new List<LarprunPreReg>();

    public virtual ICollection<Larprun> Larpruns { get; } = new List<Larprun>();

    public virtual Pronoun Pronouns { get; set; }

    public virtual ICollection<SheetUsersContact> SheetUsersContactCreatedbyusers { get; } = new List<SheetUsersContact>();

    public virtual ICollection<SheetUsersContact> SheetUsersContactUsers { get; } = new List<SheetUsersContact>();

    public virtual ICollection<UserLarprole> UserLarproles { get; } = new List<UserLarprole>();
}
