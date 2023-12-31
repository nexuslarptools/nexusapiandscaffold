using Microsoft.EntityFrameworkCore;

namespace NEXUSDataLayerScaffold.Models;

public partial class NexusLARPContextBase : DbContext
{
    public NexusLARPContextBase()
    {
    }

    public NexusLARPContextBase(DbContextOptions<NexusLARPContextBase> options)
        : base(options)
    {
    }

    public virtual DbSet<CharacterSheet> CharacterSheet { get; set; }
    public virtual DbSet<CharacterSheetApproved> CharacterSheetApproved { get; set; }
    public virtual DbSet<CharacterSheetVersion> CharacterSheetVersion { get; set; }
    public virtual DbSet<ItemSheet> ItemSheet { get; set; }
    public virtual DbSet<ItemSheetApproved> ItemSheetApproved { get; set; }
    public virtual DbSet<ItemSheetVersion> ItemSheetVersion { get; set; }
    public virtual DbSet<Larps> Larps { get; set; }
    public virtual DbSet<Larptags> Larptags { get; set; }
    public virtual DbSet<Pronouns> Pronouns { get; set; }
    public virtual DbSet<Roles> Roles { get; set; }
    public virtual DbSet<Series> Series { get; set; }
    public virtual DbSet<TagTypes> TagTypes { get; set; }
    public virtual DbSet<Tags> Tags { get; set; }
    public virtual DbSet<UserLarproles> UserLarproles { get; set; }
    public virtual DbSet<Users> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http: //go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=NexusLARP;Username=postgres;Password=L4RPEverywhere!");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<CharacterSheet>(entity =>
        {
            entity.HasIndex(e => new { e.Name, e.Seriesguid })
                .HasName("CharacterSheet_name_seriesguid_key")
                .IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");

            entity.Property(e => e.Fields)
                .HasColumnName("fields")
                .HasColumnType("jsonb");

            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");

            entity.Property(e => e.Firstapprovaldate).HasColumnName("firstapprovaldate");

            entity.Property(e => e.Gmnotes)
                .HasColumnName("gmnotes")
                .HasMaxLength(100000);

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Img1)
                .HasColumnName("img1")
                .HasMaxLength(1000);

            entity.Property(e => e.Img2)
                .HasColumnName("img2")
                .HasMaxLength(1000);

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Reason4edit)
                .HasColumnName("reason4edit")
                .HasMaxLength(2000);

            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");

            entity.Property(e => e.Secondapprovaldate).HasColumnName("secondapprovaldate");

            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .HasDefaultValueSql("1");

            entity.HasOne(d => d.Createdbyuser)
                .WithMany(p => p.CharacterSheetCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("CharacterSheet_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser)
                .WithMany(p => p.CharacterSheetFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("CharacterSheet_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser)
                .WithMany(p => p.CharacterSheetSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("CharacterSheet_secondapprovalby_fkey");

            entity.HasOne(d => d.Series)
                .WithMany(p => p.CharacterSheets)
                .HasForeignKey(d => d.Seriesguid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_series_guid");
        });

        modelBuilder.Entity<CharacterSheetApproved>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CharactersheetId).HasColumnName("charactersheet_id");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");

            entity.Property(e => e.Fields)
                .HasColumnName("fields")
                .HasColumnType("jsonb");

            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");

            entity.Property(e => e.Firstapprovaldate).HasColumnName("firstapprovaldate");

            entity.Property(e => e.Gmnotes)
                .HasColumnName("gmnotes")
                .HasMaxLength(100000);

            entity.Property(e => e.Guid).HasColumnName("guid");

            entity.Property(e => e.Img1)
                .HasColumnName("img1")
                .HasMaxLength(1000);

            entity.Property(e => e.Img2)
                .HasColumnName("img2")
                .HasMaxLength(1000);

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Reason4edit)
                .HasColumnName("reason4edit")
                .HasMaxLength(2000);

            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");

            entity.Property(e => e.Secondapprovaldate).HasColumnName("secondapprovaldate");

            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .HasDefaultValueSql("1");

            entity.HasOne(d => d.Charactersheet)
                .WithMany(p => p.CharacterSheetApproveds)
                .HasForeignKey(d => d.CharactersheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CharacterSheetApproved_charactersheet_id_fkey");

            entity.HasOne(d => d.Createdbyuser)
                .WithMany(p => p.CharacterSheetApprovedCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("CharacterSheetApproved_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser)
                .WithMany(p => p.CharacterSheetApprovedFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetApproved_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser)
                .WithMany(p => p.CharacterSheetApprovedSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetApproved_secondapprovalby_fkey");

            entity.HasOne(d => d.Series)
                .WithMany(p => p.CharacterSheetApproveds)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid");
        });

        modelBuilder.Entity<CharacterSheetVersion>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CharactersheetId).HasColumnName("charactersheet_id");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");

            entity.Property(e => e.Fields)
                .HasColumnName("fields")
                .HasColumnType("jsonb");

            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");

            entity.Property(e => e.Firstapprovaldate).HasColumnName("firstapprovaldate");

            entity.Property(e => e.Gmnotes)
                .HasColumnName("gmnotes")
                .HasMaxLength(100000);

            entity.Property(e => e.Guid).HasColumnName("guid");

            entity.Property(e => e.Img1)
                .HasColumnName("img1")
                .HasMaxLength(1000);

            entity.Property(e => e.Img2)
                .HasColumnName("img2")
                .HasMaxLength(1000);

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Reason4edit)
                .HasColumnName("reason4edit")
                .HasMaxLength(2000);

            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");

            entity.Property(e => e.Secondapprovaldate).HasColumnName("secondapprovaldate");

            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .HasDefaultValueSql("1");

            entity.HasOne(d => d.Charactersheet)
                .WithMany(p => p.CharacterSheetVersions)
                .HasForeignKey(d => d.CharactersheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CharacterSheetVersion_charactersheet_id_fkey");

            entity.HasOne(d => d.Createdbyuser)
                .WithMany(p => p.CharacterSheetVersionCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("CharacterSheetVersion_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser)
                .WithMany(p => p.CharacterSheetVersionFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetVersion_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser)
                .WithMany(p => p.CharacterSheetVersionSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("CharacterSheetVersion_secondapprovalby_fkey");

            entity.HasOne(d => d.Series)
                .WithMany(p => p.CharacterSheetVersions)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid");
        });

        modelBuilder.Entity<ItemSheet>(entity =>
        {
            entity.HasIndex(e => new { e.Seriesguid, e.Name })
                .HasName("ItemSheet_seriesguid_name_key")
                .IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");

            entity.Property(e => e.Fields)
                .HasColumnName("fields")
                .HasColumnType("jsonb");

            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");

            entity.Property(e => e.Firstapprovaldate).HasColumnName("firstapprovaldate");

            entity.Property(e => e.Gmnotes)
                .HasColumnName("gmnotes")
                .HasMaxLength(1000000);

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Img1)
                .HasColumnName("img1")
                .HasMaxLength(1000);

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Reason4edit)
                .HasColumnName("reason4edit")
                .HasMaxLength(2000);

            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");

            entity.Property(e => e.Secondapprovaldate).HasColumnName("secondapprovaldate");

            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .HasDefaultValueSql("1");

            entity.HasOne(d => d.Createdbyuser)
                .WithMany(p => p.ItemSheetCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("ItemSheet_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser)
                .WithMany(p => p.ItemSheetFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("ItemSheet_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser)
                .WithMany(p => p.ItemSheetSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("ItemSheet_secondapprovalby_fkey");

            entity.HasOne(d => d.Series)
                .WithMany(p => p.ItemSheets)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid_item");
        });

        modelBuilder.Entity<ItemSheetApproved>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");

            entity.Property(e => e.Fields)
                .HasColumnName("fields")
                .HasColumnType("jsonb");

            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");

            entity.Property(e => e.Firstapprovaldate).HasColumnName("firstapprovaldate");

            entity.Property(e => e.Gmnotes)
                .HasColumnName("gmnotes")
                .HasMaxLength(1000000);

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Img1)
                .HasColumnName("img1")
                .HasMaxLength(1000);

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.ItemsheetId).HasColumnName("itemsheet_id");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Reason4edit)
                .HasColumnName("reason4edit")
                .HasMaxLength(2000);

            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");

            entity.Property(e => e.Secondapprovaldate).HasColumnName("secondapprovaldate");

            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .HasDefaultValueSql("1");

            entity.HasOne(d => d.Createdbyuser)
                .WithMany(p => p.ItemSheetApprovedCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("ItemSheetApproved_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser)
                .WithMany(p => p.ItemSheetApprovedFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("ItemSheetApproved_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser)
                .WithMany(p => p.ItemSheetApprovedSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("ItemSheetApproved_secondapprovalby_fkey");

            entity.HasOne(d => d.Series)
                .WithMany(p => p.ItemSheetApproveds)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid_itemappr");
        });

        modelBuilder.Entity<ItemSheetVersion>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedbyuserGuid).HasColumnName("createdbyuser_guid");

            entity.Property(e => e.Fields)
                .HasColumnName("fields")
                .HasColumnType("jsonb");

            entity.Property(e => e.FirstapprovalbyuserGuid).HasColumnName("firstapprovalbyuser_guid");

            entity.Property(e => e.Firstapprovaldate).HasColumnName("firstapprovaldate");

            entity.Property(e => e.Gmnotes)
                .HasColumnName("gmnotes")
                .HasMaxLength(1000000);

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Img1)
                .HasColumnName("img1")
                .HasMaxLength(1000);

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.ItemsheetId).HasColumnName("itemsheet_id");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Reason4edit)
                .HasColumnName("reason4edit")
                .HasMaxLength(2000);

            entity.Property(e => e.SecondapprovalbyuserGuid).HasColumnName("secondapprovalbyuser_guid");

            entity.Property(e => e.Secondapprovaldate).HasColumnName("secondapprovaldate");

            entity.Property(e => e.Seriesguid).HasColumnName("seriesguid");

            entity.Property(e => e.Version)
                .HasColumnName("version")
                .HasDefaultValueSql("1");

            entity.HasOne(d => d.Createdbyuser)
                .WithMany(p => p.ItemSheetVersionCreatedbyusers)
                .HasForeignKey(d => d.CreatedbyuserGuid)
                .HasConstraintName("ItemSheetVersion_createdby_fkey");

            entity.HasOne(d => d.Firstapprovalbyuser)
                .WithMany(p => p.ItemSheetVersionFirstapprovalbyusers)
                .HasForeignKey(d => d.FirstapprovalbyuserGuid)
                .HasConstraintName("ItemSheetVersion_firstapprovalby_fkey");

            entity.HasOne(d => d.Secondapprovalbyuser)
                .WithMany(p => p.ItemSheetVersionSecondapprovalbyusers)
                .HasForeignKey(d => d.SecondapprovalbyuserGuid)
                .HasConstraintName("ItemSheetVersion_secondapprovalby_fkey");

            entity.HasOne(d => d.Series)
                .WithMany(p => p.ItemSheetVersions)
                .HasForeignKey(d => d.Seriesguid)
                .HasConstraintName("fk_series_guid_itemvers");
        });

        modelBuilder.Entity<Larps>(entity =>
        {
            entity.HasKey(e => e.Guid)
                .HasName("larps_guid");

            entity.ToTable("LARPs");

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Location)
                .HasColumnName("location")
                .HasMaxLength(1000);

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Shortname)
                .HasColumnName("shortname")
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Larptags>(entity =>
        {
            entity.ToTable("LARPTags");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Isactive)
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Larpguid).HasColumnName("larpguid");

            entity.Property(e => e.Tagguid).HasColumnName("tagguid");

            entity.HasOne(d => d.Larpgu)
                .WithMany(p => p.Larptags)
                .HasForeignKey(d => d.Larpguid)
                .HasConstraintName("LARPTags_larpguid_fkey");

            entity.HasOne(d => d.Taggu)
                .WithMany(p => p.Larptags)
                .HasForeignKey(d => d.Tagguid)
                .HasConstraintName("LARPTags_tagguid_fkey");
        });

        modelBuilder.Entity<Pronouns>(entity =>
        {
            entity.HasKey(e => e.Guid)
                .HasName("pronouns_guid");

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Pronouns1)
                .HasColumnName("pronouns")
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Rolename)
                .HasColumnName("rolename")
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Guid)
                .HasName("series_guid");

            entity.HasIndex(e => e.Title)
                .HasName("Series_title_key")
                .IsUnique();

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Createdate)
                .HasColumnName("createdate")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.Deactivedate).HasColumnName("deactivedate");

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Tags)
                .HasColumnName("tags")
                .HasColumnType("jsonb");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title")
                .HasMaxLength(1000);

            entity.Property(e => e.Titlejpn)
                .HasColumnName("titlejpn")
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<TagTypes>(entity =>
        {
            entity.HasKey(e => e.Guid)
                .HasName("tagtypes_guid");

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Tags>(entity =>
        {
            entity.HasKey(e => e.Guid)
                .HasName("tags_guid");

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Isactive)
                .IsRequired()
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(1000);

            entity.Property(e => e.Tagtypeguid).HasColumnName("tagtypeguid");

            entity.HasOne(d => d.Tagtypegu)
                .WithMany(p => p.Tags)
                .HasForeignKey(d => d.Tagtypeguid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tagtype_guid_tags");
        });

        modelBuilder.Entity<UserLarproles>(entity =>
        {
            entity.ToTable("UserLARPRoles");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Isactive)
                .HasColumnName("isactive")
                .HasDefaultValueSql("true");

            entity.Property(e => e.Larpguid).HasColumnName("larpguid");

            entity.Property(e => e.Roleid).HasColumnName("roleid");

            entity.Property(e => e.Userguid).HasColumnName("userguid");

            entity.HasOne(d => d.Larpgu)
                .WithMany(p => p.UserLarproles)
                .HasForeignKey(d => d.Larpguid)
                .HasConstraintName("UserLARPRoles_larpguid_fkey");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.UserLarproles)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("UserLARPRoles_roleid_fkey");

            entity.HasOne(d => d.Usergu)
                .WithMany(p => p.UserLarproles)
                .HasForeignKey(d => d.Userguid)
                .HasConstraintName("UserLARPRoles_userguid_fkey");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Guid)
                .HasName("users_guid");

            entity.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasDefaultValueSql("uuid_generate_v1()");

            entity.Property(e => e.Authid)
                .HasColumnName("authid")
                .HasMaxLength(200);

            entity.Property(e => e.Discordname)
                .HasColumnName("discordname")
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(1000);

            entity.Property(e => e.Firstname)
                .HasColumnName("firstname")
                .HasMaxLength(1000);

            entity.Property(e => e.Lastname)
                .HasColumnName("lastname")
                .HasMaxLength(1000);

            entity.Property(e => e.Preferredname)
                .HasColumnName("preferredname")
                .HasMaxLength(1000);

            entity.Property(e => e.Pronounsguid).HasColumnName("pronounsguid");

            entity.HasOne(d => d.Pronounsgu)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.Pronounsguid)
                .HasConstraintName("fk_pronouns_guid_user");

            entity.Property(e => e.Isactive)
                .HasColumnName("isactive");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}