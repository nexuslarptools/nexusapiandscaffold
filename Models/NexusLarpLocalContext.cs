using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NEXUSDataLayerScaffold.Models;

public partial class NexusLarpLocalContext : DbContext
{
    public NexusLarpLocalContext()
    {
    }

    public NexusLarpLocalContext(DbContextOptions<NexusLarpLocalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CharacterSheet> CharacterSheets { get; set; }

    public virtual DbSet<CharacterSheetApproved> CharacterSheetApproveds { get; set; }

    public virtual DbSet<CharacterSheetVersion> CharacterSheetVersions { get; set; }

    public virtual DbSet<ItemSheet> ItemSheets { get; set; }

    public virtual DbSet<ItemSheetApproved> ItemSheetApproveds { get; set; }

    public virtual DbSet<ItemSheetVersion> ItemSheetVersions { get; set; }

    public virtual DbSet<ItemUsersContact> ItemUsersContacts { get; set; }

    public virtual DbSet<Larp> Larps { get; set; }

    public virtual DbSet<LarpplayerCharacterSheetAllowed> LarpplayerCharacterSheetAlloweds { get; set; }

    public virtual DbSet<LarpplayerCharacterSheetDisllowed> LarpplayerCharacterSheetDislloweds { get; set; }

    public virtual DbSet<LarpplayerSeriesAllowed> LarpplayerSeriesAlloweds { get; set; }

    public virtual DbSet<LarpplayerSeriesDisllowed> LarpplayerSeriesDislloweds { get; set; }

    public virtual DbSet<LarpplayerTagAllowed> LarpplayerTagAlloweds { get; set; }

    public virtual DbSet<LarpplayerTagDisllowed> LarpplayerTagDislloweds { get; set; }

    public virtual DbSet<Larprun> Larpruns { get; set; }

    public virtual DbSet<LarprunPreReg> LarprunPreRegs { get; set; }

    public virtual DbSet<Larptag> Larptags { get; set; }

    public virtual DbSet<Pronoun> Pronouns { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Series> Series { get; set; }

    public virtual DbSet<SheetUsersContact> SheetUsersContacts { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagType> TagTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserLarprole> UserLarproles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<CharacterSheet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("csheet_guid");

            entity.ToTable("CharacterSheet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.EditbyUserGuid).HasColumnName("editby_user_guid");
            entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("fields");
            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");
            entity.Property(e => e.Firstapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("firstapprovaldate");
            entity.Property(e => e.Gmnotes)
                .HasMaxLength(100000)
                .HasColumnName("gmnotes");
            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Img1)
                .HasMaxLength(1000)
                .HasColumnName("img1");
            entity.Property(e => e.Img2)
                .HasMaxLength(1000)
                .HasColumnName("img2");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Reason4edit)
                .HasMaxLength(2000)
                .HasColumnName("reason4edit");
            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");
            entity.Property(e => e.Secondapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("secondapprovaldate");
            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");
            entity.Property(e => e.Version)
                .HasDefaultValueSql("1")
                .HasColumnName("version");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.CharacterSheetCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("CharacterSheet_createdby_fkey");

            entity.HasOne(d => d.EditbyUser).WithMany(p => p.CharacterSheetEditbyUsers)
                .HasForeignKey(d => d.EditbyUserGuid)
                .HasConstraintName("CharacterSheet_editby_user_guid_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser).WithMany(p => p.CharacterSheetFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("CharacterSheet_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser).WithMany(p => p.CharacterSheetSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("CharacterSheet_secondapprovalby_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.CharacterSheets)
                .HasForeignKey(d => d.Seriesguid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_series_guid");
        });

        modelBuilder.Entity<CharacterSheetApproved>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("csheetapprov_guid");

            entity.ToTable("CharacterSheetApproved");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CharactersheetId).HasColumnName("charactersheet_id");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.EditbyUserGuid).HasColumnName("editby_user_guid");
            entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("fields");
            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");
            entity.Property(e => e.Firstapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("firstapprovaldate");
            entity.Property(e => e.Gmnotes)
                .HasMaxLength(100000)
                .HasColumnName("gmnotes");
            entity.Property(e => e.Guid).HasColumnName("guid");
            entity.Property(e => e.Img1)
                .HasMaxLength(1000)
                .HasColumnName("img1");
            entity.Property(e => e.Img2)
                .HasMaxLength(1000)
                .HasColumnName("img2");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Reason4edit)
                .HasMaxLength(2000)
                .HasColumnName("reason4edit");
            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");
            entity.Property(e => e.Secondapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("secondapprovaldate");
            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");
            entity.Property(e => e.Version)
                .HasDefaultValueSql("1")
                .HasColumnName("version");

            entity.HasOne(d => d.Charactersheet).WithMany(p => p.CharacterSheetApproveds)
                .HasForeignKey(d => d.CharactersheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CharacterSheetApproved_charactersheet_id_fkey");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.CharacterSheetApprovedCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("CharacterSheetApproved_createdby_fkey");

            entity.HasOne(d => d.EditbyUser).WithMany(p => p.CharacterSheetApprovedEditbyUsers)
                .HasForeignKey(d => d.EditbyUserGuid)
                .HasConstraintName("CharacterSheetApproved_editby_user_guid_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser).WithMany(p => p.CharacterSheetApprovedFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetApproved_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser).WithMany(p => p.CharacterSheetApprovedSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetApproved_secondapprovalby_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.CharacterSheetApproveds)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid");
        });

        modelBuilder.Entity<CharacterSheetVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("csheetvers_guid");

            entity.ToTable("CharacterSheetVersion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CharactersheetId).HasColumnName("charactersheet_id");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("fields");
            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");
            entity.Property(e => e.Firstapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("firstapprovaldate");
            entity.Property(e => e.Gmnotes)
                .HasMaxLength(100000)
                .HasColumnName("gmnotes");
            entity.Property(e => e.Guid).HasColumnName("guid");
            entity.Property(e => e.Img1)
                .HasMaxLength(1000)
                .HasColumnName("img1");
            entity.Property(e => e.Img2)
                .HasMaxLength(1000)
                .HasColumnName("img2");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Reason4edit)
                .HasMaxLength(2000)
                .HasColumnName("reason4edit");
            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");
            entity.Property(e => e.Secondapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("secondapprovaldate");
            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");
            entity.Property(e => e.Version)
                .HasDefaultValueSql("1")
                .HasColumnName("version");

            entity.HasOne(d => d.Charactersheet).WithMany(p => p.CharacterSheetVersions)
                .HasForeignKey(d => d.CharactersheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CharacterSheetVersion_charactersheet_id_fkey");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.CharacterSheetVersionCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("CharacterSheetVersion_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser).WithMany(p => p.CharacterSheetVersionFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetVersion_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser).WithMany(p => p.CharacterSheetVersionSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetVersion_secondapprovalby_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.CharacterSheetVersions)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid");
        });

        modelBuilder.Entity<ItemSheet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("isheet_guid");

            entity.ToTable("ItemSheet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.EditbyUserGuid).HasColumnName("editby_user_guid");
            entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("fields");
            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");
            entity.Property(e => e.Firstapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("firstapprovaldate");
            entity.Property(e => e.Gmnotes)
                .HasMaxLength(1000000)
                .HasColumnName("gmnotes");
            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Img1)
                .HasMaxLength(1000)
                .HasColumnName("img1");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Reason4edit)
                .HasMaxLength(2000)
                .HasColumnName("reason4edit");
            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");
            entity.Property(e => e.Secondapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("secondapprovaldate");
            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");
            entity.Property(e => e.Version)
                .HasDefaultValueSql("1")
                .HasColumnName("version");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.ItemSheetCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("ItemSheet_createdby_fkey");

            entity.HasOne(d => d.EditbyUser).WithMany(p => p.ItemSheetEditbyUsers)
                .HasForeignKey(d => d.EditbyUserGuid)
                .HasConstraintName("ItemSheet_editby_user_guid_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser).WithMany(p => p.ItemSheetFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("ItemSheet_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser).WithMany(p => p.ItemSheetSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("ItemSheet_secondapprovalby_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.ItemSheets)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid_item");
        });

        modelBuilder.Entity<ItemSheetApproved>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("isheetapp_guid");

            entity.ToTable("ItemSheetApproved");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.EditbyUserGuid).HasColumnName("editby_user_guid");
            entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("fields");
            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");
            entity.Property(e => e.Firstapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("firstapprovaldate");
            entity.Property(e => e.Gmnotes)
                .HasMaxLength(1000000)
                .HasColumnName("gmnotes");
            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Img1)
                .HasMaxLength(1000)
                .HasColumnName("img1");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.ItemsheetId).HasColumnName("itemsheet_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Reason4edit)
                .HasMaxLength(2000)
                .HasColumnName("reason4edit");
            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");
            entity.Property(e => e.Secondapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("secondapprovaldate");
            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");
            entity.Property(e => e.Version)
                .HasDefaultValueSql("1")
                .HasColumnName("version");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.ItemSheetApprovedCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("ItemSheetApproved_createdby_fkey");

            entity.HasOne(d => d.EditbyUser).WithMany(p => p.ItemSheetApprovedEditbyUsers)
                .HasForeignKey(d => d.EditbyUserGuid)
                .HasConstraintName("ItemSheetApproved_editby_user_guid_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser).WithMany(p => p.ItemSheetApprovedFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("ItemSheetApproved_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser).WithMany(p => p.ItemSheetApprovedSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("ItemSheetApproved_secondapprovalby_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.ItemSheetApproveds)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid_itemappr");
        });

        modelBuilder.Entity<ItemSheetVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("isheetvers_guid");

            entity.ToTable("ItemSheetVersion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("fields");
            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");
            entity.Property(e => e.Firstapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("firstapprovaldate");
            entity.Property(e => e.Gmnotes)
                .HasMaxLength(1000000)
                .HasColumnName("gmnotes");
            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Img1)
                .HasMaxLength(1000)
                .HasColumnName("img1");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.ItemsheetId).HasColumnName("itemsheet_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Reason4edit)
                .HasMaxLength(2000)
                .HasColumnName("reason4edit");
            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");
            entity.Property(e => e.Secondapprovaldate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("secondapprovaldate");
            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");
            entity.Property(e => e.Version)
                .HasDefaultValueSql("1")
                .HasColumnName("version");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.ItemSheetVersionCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("ItemSheetVersion_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser).WithMany(p => p.ItemSheetVersionFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("ItemSheetVersion_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser).WithMany(p => p.ItemSheetVersionSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("ItemSheetVersion_secondapprovalby_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.ItemSheetVersions)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid_itemvers");
        });

        modelBuilder.Entity<ItemUsersContact>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("itemusers_contacts_guid");

            entity.ToTable("ItemUsers_Contacts");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Createddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createddate");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.ItemsheetGuid).HasColumnName("itemsheet_guid");
            entity.Property(e => e.UserGuid).HasColumnName("user_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.ItemUsersContactCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_itemuser_contact_createdby_userguid");

            entity.HasOne(d => d.User).WithMany(p => p.ItemUsersContactUsers)
                .HasForeignKey(d => d.UserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_itemuser_contact_userguid");
        });

        modelBuilder.Entity<Larp>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larps_guid");

            entity.ToTable("LARPs");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Location)
                .HasMaxLength(1000)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Shortname)
                .HasMaxLength(1000)
                .HasColumnName("shortname");
        });

        modelBuilder.Entity<LarpplayerCharacterSheetAllowed>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpplayercharactersheetallowed_id");

            entity.ToTable("LARPPlayerCharacterSheetAllowed");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.CharactersheetGuid).HasColumnName("charactersheet_guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.LarpplayerCharacterSheetAlloweds)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerCharacterSheetAllowed_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.LarpplayerCharacterSheetAlloweds)
                .HasForeignKey(d => d.LarpGuid)
                .HasConstraintName("LARPPlayerCharacterSheetAllowed_larp_guid_fkey");
        });

        modelBuilder.Entity<LarpplayerCharacterSheetDisllowed>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpplayercharactersheetdisllowed_id");

            entity.ToTable("LARPPlayerCharacterSheetDisllowed");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.CharactersheetGuid).HasColumnName("charactersheet_guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.LarpplayerCharacterSheetDislloweds)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerCharacterSheetDisllowed_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.LarpplayerCharacterSheetDislloweds)
                .HasForeignKey(d => d.LarpGuid)
                .HasConstraintName("LARPPlayerCharacterSheetDisllowed_larp_guid_fkey");
        });

        modelBuilder.Entity<LarpplayerSeriesAllowed>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpplayerseriesallowed_id");

            entity.ToTable("LARPPlayerSeriesAllowed");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");
            entity.Property(e => e.SeriesGuid).HasColumnName("series_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.LarpplayerSeriesAlloweds)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerSeriesAllowed_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.LarpplayerSeriesAlloweds)
                .HasForeignKey(d => d.LarpGuid)
                .HasConstraintName("LARPPlayerSeriesAllowed_larp_guid_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.LarpplayerSeriesAlloweds)
                .HasForeignKey(d => d.SeriesGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerSeriesAllowed_series_guid_fkey");
        });

        modelBuilder.Entity<LarpplayerSeriesDisllowed>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpplayerseriesdisllowed_id");

            entity.ToTable("LARPPlayerSeriesDisllowed");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");
            entity.Property(e => e.SeriesGuid).HasColumnName("series_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.LarpplayerSeriesDislloweds)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerSeriesDisllowed_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.LarpplayerSeriesDislloweds)
                .HasForeignKey(d => d.LarpGuid)
                .HasConstraintName("LARPPlayerSeriesDisllowed_larp_guid_fkey");

            entity.HasOne(d => d.Series).WithMany(p => p.LarpplayerSeriesDislloweds)
                .HasForeignKey(d => d.SeriesGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerSeriesDisllowed_series_guid_fkey");
        });

        modelBuilder.Entity<LarpplayerTagAllowed>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpplayertagallowed_id");

            entity.ToTable("LARPPlayerTagAllowed");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");
            entity.Property(e => e.TagGuid).HasColumnName("tag_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.LarpplayerTagAlloweds)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerTagAllowed_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.LarpplayerTagAlloweds)
                .HasForeignKey(d => d.LarpGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerTagAllowed_larp_guid_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.LarpplayerTagAlloweds)
                .HasForeignKey(d => d.TagGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerTagAllowed_tag_guid_fkey");
        });

        modelBuilder.Entity<LarpplayerTagDisllowed>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpplayertagdisllowed_id");

            entity.ToTable("LARPPlayerTagDisllowed");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");
            entity.Property(e => e.TagGuid).HasColumnName("tag_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.LarpplayerTagDislloweds)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerTagDisllowed_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.LarpplayerTagDislloweds)
                .HasForeignKey(d => d.LarpGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerTagDisllowed_larp_guid_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.LarpplayerTagDislloweds)
                .HasForeignKey(d => d.TagGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPPlayerTagDisllowed_tag_guid_fkey");
        });

        modelBuilder.Entity<Larprun>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larpruns_id");

            entity.ToTable("LARPRuns");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarpGuid).HasColumnName("larp_guid");
            entity.Property(e => e.Larprunenddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("larprunenddate");
            entity.Property(e => e.Larprunstartdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("larprunstartdate");
            entity.Property(e => e.Preregenddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("preregenddate");
            entity.Property(e => e.Preregstartdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("preregstartdate");
            entity.Property(e => e.Runname)
                .HasMaxLength(2000)
                .HasColumnName("runname");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.Larpruns)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPRuns_createdbyuser_guid_fkey");

            entity.HasOne(d => d.Larp).WithMany(p => p.Larpruns)
                .HasForeignKey(d => d.LarpGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPRuns_larp_guid_fkey");
        });

        modelBuilder.Entity<LarprunPreReg>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("larprunprereg_id");

            entity.ToTable("LARPRunPreReg");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.CharactersheetChoice1).HasColumnName("charactersheet_choice1");
            entity.Property(e => e.CharactersheetChoice2).HasColumnName("charactersheet_choice2");
            entity.Property(e => e.CharactersheetChoice3).HasColumnName("charactersheet_choice3");
            entity.Property(e => e.CharactersheetCustomchoice1)
                .HasMaxLength(2000)
                .HasColumnName("charactersheet_customchoice1");
            entity.Property(e => e.CharactersheetCustomchoice1Series)
                .HasMaxLength(2000)
                .HasColumnName("charactersheet_customchoice1_series");
            entity.Property(e => e.CharactersheetCustomchoice2)
                .HasMaxLength(2000)
                .HasColumnName("charactersheet_customchoice2");
            entity.Property(e => e.CharactersheetCustomchoice2Series)
                .HasMaxLength(2000)
                .HasColumnName("charactersheet_customchoice2_series");
            entity.Property(e => e.CharactersheetCustomchoice3)
                .HasMaxLength(2000)
                .HasColumnName("charactersheet_customchoice3");
            entity.Property(e => e.CharactersheetCustomchoice3Series)
                .HasMaxLength(2000)
                .HasColumnName("charactersheet_customchoice3_series");
            entity.Property(e => e.CharactersheetRegistered).HasColumnName("charactersheet_registered");
            entity.Property(e => e.CharactersheetRegisteredApprovedbyUser).HasColumnName("charactersheet_registered_approvedby_user");
            entity.Property(e => e.CharactersheetRegisteredApprovedsheet).HasColumnName("charactersheet_registered_approvedsheet");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.LarprunGuid).HasColumnName("larprun_guid");
            entity.Property(e => e.UserGuid).HasColumnName("user_guid");

            entity.HasOne(d => d.CharactersheetRegisteredApprovedbyUserNavigation).WithMany(p => p.LarprunPreRegCharactersheetRegisteredApprovedbyUserNavigations)
                .HasForeignKey(d => d.CharactersheetRegisteredApprovedbyUser)
                .HasConstraintName("LARPRunPreReg_charactersheet_registered_approvedby_user_fkey");

            entity.HasOne(d => d.Larprun).WithMany(p => p.LarprunPreRegs)
                .HasForeignKey(d => d.LarprunGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPRunPreReg_larprun_guid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.LarprunPreRegUsers)
                .HasForeignKey(d => d.UserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LARPRunPreReg_user_guid_fkey");
        });

        modelBuilder.Entity<Larptag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("LARPTags_pkey");

            entity.ToTable("LARPTags");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Isactive)
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Larpguid).HasColumnName("larpguid");
            entity.Property(e => e.Tagguid).HasColumnName("tagguid");

            entity.HasOne(d => d.Larp).WithMany(p => p.Larptags)
                .HasForeignKey(d => d.Larpguid)
                .HasConstraintName("LARPTags_larpguid_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.Larptags)
                .HasForeignKey(d => d.Tagguid)
                .HasConstraintName("LARPTags_tagguid_fkey");
        });

        modelBuilder.Entity<Pronoun>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("pronouns_guid");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Pronouns)
                .HasMaxLength(1000)
                .HasColumnName("pronouns");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Roles_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ord).HasColumnName("ord");
            entity.Property(e => e.Rolename)
                .HasMaxLength(200)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("series_guid");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Createdate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.Deactivedate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deactivedate");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Tags)
                .HasColumnType("jsonb")
                .HasColumnName("tags");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("title");
            entity.Property(e => e.Titlejpn)
                .HasMaxLength(1000)
                .HasColumnName("titlejpn");
        });

        modelBuilder.Entity<SheetUsersContact>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("sheetusers_contacts_guid");

            entity.ToTable("SheetUsers_Contacts");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.CharactersheetGuid).HasColumnName("charactersheet_guid");
            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");
            entity.Property(e => e.Createddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createddate");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.UserGuid).HasColumnName("user_guid");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.SheetUsersContactCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sheetuser_contact_createdby_userguid");

            entity.HasOne(d => d.User).WithMany(p => p.SheetUsersContactUsers)
                .HasForeignKey(d => d.UserGuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sheetuser_contact_userguid");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("tags_guid");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .HasColumnName("name");
            entity.Property(e => e.Tagtypeguid).HasColumnName("tagtypeguid");

            entity.HasOne(d => d.Tagtype).WithMany(p => p.Tags)
                .HasForeignKey(d => d.Tagtypeguid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tagtype_guid_tags");
        });

        modelBuilder.Entity<TagType>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("tagtypes_guid");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("users_guid");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("uuid_generate_v1()")
                .HasColumnName("guid");
            entity.Property(e => e.Authid)
                .HasMaxLength(200)
                .HasColumnName("authid");
            entity.Property(e => e.Discordname)
                .HasMaxLength(200)
                .HasColumnName("discordname");
            entity.Property(e => e.Email)
                .HasMaxLength(1000)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(1000)
                .HasColumnName("firstname");
            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Lastname)
                .HasMaxLength(1000)
                .HasColumnName("lastname");
            entity.Property(e => e.Preferredname)
                .HasMaxLength(1000)
                .HasColumnName("preferredname");
            entity.Property(e => e.Pronounsguid).HasColumnName("pronounsguid");

            entity.HasOne(d => d.Pronouns).WithMany(p => p.Users)
                .HasForeignKey(d => d.Pronounsguid)
                .HasConstraintName("fk_pronouns_guid_user");
        });

        modelBuilder.Entity<UserLarprole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserLARPRoles_pkey");

            entity.ToTable("UserLARPRoles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Isactive)
                .HasDefaultValueSql("true")
                .HasColumnName("isactive");
            entity.Property(e => e.Larpguid).HasColumnName("larpguid");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Userguid).HasColumnName("userguid");

            entity.HasOne(d => d.Larp).WithMany(p => p.UserLarproles)
                .HasForeignKey(d => d.Larpguid)
                .HasConstraintName("UserLARPRoles_larpguid_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.UserLarproles)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("UserLARPRoles_roleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserLarproles)
                .HasForeignKey(d => d.Userguid)
                .HasConstraintName("UserLARPRoles_userguid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
