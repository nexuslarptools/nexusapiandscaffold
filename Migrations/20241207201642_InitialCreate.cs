using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NEXUSDataLayerScaffold.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "ItemTypes",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    Type = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemtypes_guid", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "LARPs",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    shortname = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    location = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larps_guid", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "Pronouns",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    pronouns = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pronouns_guid", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolename = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ord = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Roles_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    titlejpn = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tags = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    deactivedate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("series_guid", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "TagTypes",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tagtypes_guid", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    firstname = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    lastname = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    preferredname = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    email = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    pronounsguid = table.Column<Guid>(type: "uuid", nullable: true),
                    discordname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    authid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fk_pronouns_guid_user",
                        column: x => x.pronounsguid,
                        principalTable: "Pronouns",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    seriesguid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    img1 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    img2 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fields = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    secondapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    secondapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gmnotes = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: true),
                    reason4edit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1"),
                    editby_user_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    taglists = table.Column<string>(type: "json", nullable: true),
                    readyforapproval = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("csheet_guid", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheet_createdby_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheet_editby_user_guid_fkey",
                        column: x => x.editby_user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheet_firstapprovalby_fkey",
                        column: x => x.firstapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheet_secondapprovalby_fkey",
                        column: x => x.secondapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_series_guid",
                        column: x => x.seriesguid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheetReviewMessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charactersheet_id = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("charactersheetreviewmessages_id", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetReviewMessages_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    seriesguid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    img1 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fields = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    secondapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    secondapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gmnotes = table.Column<string>(type: "character varying(1000000)", maxLength: 1000000, nullable: true),
                    reason4edit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "1"),
                    editby_user_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    taglists = table.Column<string>(type: "json", nullable: true),
                    readyforapproval = table.Column<bool>(type: "boolean", nullable: false),
                    isdoubleside = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    fields2ndside = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    itemtype_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("isheet_guid", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheet_ItemType_fkey",
                        column: x => x.itemtype_guid,
                        principalTable: "ItemTypes",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheet_createdby_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheet_editby_user_guid_fkey",
                        column: x => x.editby_user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheet_firstapprovalby_fkey",
                        column: x => x.firstapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheet_secondapprovalby_fkey",
                        column: x => x.secondapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_series_guid_item",
                        column: x => x.seriesguid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetApproved",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemsheet_id = table.Column<int>(type: "integer", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    seriesguid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    img1 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fields = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    secondapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    secondapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gmnotes = table.Column<string>(type: "character varying(1000000)", maxLength: 1000000, nullable: true),
                    reason4edit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "1"),
                    editby_user_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    taglists = table.Column<string>(type: "json", nullable: true),
                    isdoubleside = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    fields2ndside = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    itemtype_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("isheetapp_guid", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetApproved_ItemType_fkey",
                        column: x => x.itemtype_guid,
                        principalTable: "ItemTypes",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheetApproved_createdby_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheetApproved_editby_user_guid_fkey",
                        column: x => x.editby_user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheetApproved_firstapprovalby_fkey",
                        column: x => x.firstapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheetApproved_secondapprovalby_fkey",
                        column: x => x.secondapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_series_guid_itemappr",
                        column: x => x.seriesguid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetReviewMessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemsheet_id = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemsheetreviewmessages_id", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetReviewMessages_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetVersion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemsheet_id = table.Column<int>(type: "integer", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    seriesguid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    img1 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fields = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    secondapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    secondapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gmnotes = table.Column<string>(type: "character varying(1000000)", maxLength: 1000000, nullable: true),
                    reason4edit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("isheetvers_guid", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetVersion_createdby_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheetVersion_firstapprovalby_fkey",
                        column: x => x.firstapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "ItemSheetVersion_secondapprovalby_fkey",
                        column: x => x.secondapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_series_guid_itemvers",
                        column: x => x.seriesguid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemUsers_Contacts",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    itemsheet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: true),
                    createddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemusers_contacts_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fk_itemuser_contact_createdby_userguid",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_itemuser_contact_userguid",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPPlayerCharacterSheetAllowed",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    charactersheet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpplayercharactersheetallowed_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPPlayerCharacterSheetAllowed_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerCharacterSheetAllowed_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPPlayerCharacterSheetDisllowed",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    charactersheet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpplayercharactersheetdisllowed_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPPlayerCharacterSheetDisllowed_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerCharacterSheetDisllowed_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPPlayerSeriesAllowed",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    series_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpplayerseriesallowed_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPPlayerSeriesAllowed_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerSeriesAllowed_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerSeriesAllowed_series_guid_fkey",
                        column: x => x.series_guid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPPlayerSeriesDisllowed",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    series_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpplayerseriesdisllowed_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPPlayerSeriesDisllowed_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerSeriesDisllowed_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerSeriesDisllowed_series_guid_fkey",
                        column: x => x.series_guid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPRuns",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    runname = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    preregstartdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    preregenddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    larprunstartdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    larprunenddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpruns_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPRuns_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPRuns_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "SheetUsers_Contacts",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    charactersheet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: true),
                    createddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sheetusers_contacts_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fk_sheetuser_contact_createdby_userguid",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_sheetuser_contact_userguid",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tagtypeguid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    isapproved = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    approvedby_user_guid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tags_guid", x => x.guid);
                    table.ForeignKey(
                        name: "Tags_approvedby_fkey",
                        column: x => x.approvedby_user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_tagtype_guid_tags",
                        column: x => x.tagtypeguid,
                        principalTable: "TagTypes",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "UserLARPRoles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userguid = table.Column<Guid>(type: "uuid", nullable: true),
                    larpguid = table.Column<Guid>(type: "uuid", nullable: true),
                    roleid = table.Column<int>(type: "integer", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserLARPRoles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "UserLARPRoles_larpguid_fkey",
                        column: x => x.larpguid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "UserLARPRoles_roleid_fkey",
                        column: x => x.roleid,
                        principalTable: "Roles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "UserLARPRoles_userguid_fkey",
                        column: x => x.userguid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheetApproved",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charactersheet_id = table.Column<int>(type: "integer", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    seriesguid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    img1 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    img2 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fields = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    secondapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    secondapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gmnotes = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: true),
                    reason4edit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1"),
                    editby_user_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    taglists = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("csheetapprov_guid", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetApproved_charactersheet_id_fkey",
                        column: x => x.charactersheet_id,
                        principalTable: "CharacterSheet",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "CharacterSheetApproved_createdby_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheetApproved_editby_user_guid_fkey",
                        column: x => x.editby_user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheetApproved_firstapprovalby_fkey",
                        column: x => x.firstapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheetApproved_secondapprovalby_fkey",
                        column: x => x.secondapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_series_guid",
                        column: x => x.seriesguid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheetVersion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charactersheet_id = table.Column<int>(type: "integer", nullable: false),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    seriesguid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    img1 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    img2 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fields = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    firstapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    secondapprovalbyuser_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    secondapprovaldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gmnotes = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: true),
                    reason4edit = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("csheetvers_guid", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetVersion_charactersheet_id_fkey",
                        column: x => x.charactersheet_id,
                        principalTable: "CharacterSheet",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "CharacterSheetVersion_createdby_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheetVersion_firstapprovalby_fkey",
                        column: x => x.firstapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "CharacterSheetVersion_secondapprovalby_fkey",
                        column: x => x.secondapprovalbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "fk_series_guid",
                        column: x => x.seriesguid,
                        principalTable: "Series",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPRunPreReg",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larprun_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    charactersheet_choice1 = table.Column<Guid>(type: "uuid", nullable: true),
                    charactersheet_choice2 = table.Column<Guid>(type: "uuid", nullable: true),
                    charactersheet_choice3 = table.Column<Guid>(type: "uuid", nullable: true),
                    charactersheet_customchoice1 = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    charactersheet_customchoice1_series = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    charactersheet_customchoice2 = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    charactersheet_customchoice2_series = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    charactersheet_customchoice3 = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    charactersheet_customchoice3_series = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    charactersheet_registered = table.Column<Guid>(type: "uuid", nullable: true),
                    charactersheet_registered_approvedsheet = table.Column<bool>(type: "boolean", nullable: true),
                    charactersheet_registered_approvedby_user = table.Column<Guid>(type: "uuid", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larprunprereg_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPRunPreReg_charactersheet_registered_approvedby_user_fkey",
                        column: x => x.charactersheet_registered_approvedby_user,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPRunPreReg_larprun_guid_fkey",
                        column: x => x.larprun_guid,
                        principalTable: "LARPRuns",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPRunPreReg_user_guid_fkey",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheetTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charactersheet_id = table.Column<int>(type: "integer", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("charactersheettags_id", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetTags_CharacterSheet_Id_fkey",
                        column: x => x.charactersheet_id,
                        principalTable: "CharacterSheet",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "CharacterSheetTags_Tag_guid",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetApprovedTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemsheetapproved_id = table.Column<int>(type: "integer", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemsheetapprovedtags_id", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetApprovedTags_ItemSheetApproved_fkey",
                        column: x => x.itemsheetapproved_id,
                        principalTable: "ItemSheetApproved",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "ItemSheetApprovedTags_Tag_guid",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    itemsheet_id = table.Column<int>(type: "integer", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemsheettags_id", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetTags_ItemSheet_fkey",
                        column: x => x.itemsheet_id,
                        principalTable: "ItemSheet",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "ItemSheetTags_Tag_guid",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPPlayerTagAllowed",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpplayertagallowed_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPPlayerTagAllowed_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerTagAllowed_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerTagAllowed_tag_guid_fkey",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPPlayerTagDisllowed",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v1()"),
                    larp_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdbyuser_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("larpplayertagdisllowed_id", x => x.guid);
                    table.ForeignKey(
                        name: "LARPPlayerTagDisllowed_createdbyuser_guid_fkey",
                        column: x => x.createdbyuser_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerTagDisllowed_larp_guid_fkey",
                        column: x => x.larp_guid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPPlayerTagDisllowed_tag_guid_fkey",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "LARPTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tagguid = table.Column<Guid>(type: "uuid", nullable: true),
                    larpguid = table.Column<Guid>(type: "uuid", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("LARPTags_pkey", x => x.id);
                    table.ForeignKey(
                        name: "LARPTags_larpguid_fkey",
                        column: x => x.larpguid,
                        principalTable: "LARPs",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "LARPTags_tagguid_fkey",
                        column: x => x.tagguid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "SeriesTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    series_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("seriestags_id", x => x.id);
                    table.ForeignKey(
                        name: "SeriesTags_CharacterSheet_Guid_fkey",
                        column: x => x.series_guid,
                        principalTable: "Series",
                        principalColumn: "guid");
                    table.ForeignKey(
                        name: "SeriesTags_Tag_guid",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheetApprovedTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    charactersheetapproved_id = table.Column<int>(type: "integer", nullable: false),
                    tag_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("charactersheetapprovedtags_id", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetApprovedTags_CharacterSheet_Id_fkey",
                        column: x => x.charactersheetapproved_id,
                        principalTable: "CharacterSheetApproved",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "CharacterSheetApprovedTags_Tag_guid",
                        column: x => x.tag_guid,
                        principalTable: "Tags",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheet_createdbyuser_guid",
                table: "CharacterSheet",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheet_editby_user_guid",
                table: "CharacterSheet",
                column: "editby_user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheet_firstapprovalbyuser_guid",
                table: "CharacterSheet",
                column: "firstapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheet_secondapprovalbyuser_guid",
                table: "CharacterSheet",
                column: "secondapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheet_seriesguid",
                table: "CharacterSheet",
                column: "seriesguid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApproved_charactersheet_id",
                table: "CharacterSheetApproved",
                column: "charactersheet_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApproved_createdbyuser_guid",
                table: "CharacterSheetApproved",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApproved_editby_user_guid",
                table: "CharacterSheetApproved",
                column: "editby_user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApproved_firstapprovalbyuser_guid",
                table: "CharacterSheetApproved",
                column: "firstapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApproved_secondapprovalbyuser_guid",
                table: "CharacterSheetApproved",
                column: "secondapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApproved_seriesguid",
                table: "CharacterSheetApproved",
                column: "seriesguid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApprovedTags_charactersheetapproved_id",
                table: "CharacterSheetApprovedTags",
                column: "charactersheetapproved_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetApprovedTags_tag_guid",
                table: "CharacterSheetApprovedTags",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetReviewMessages_createdbyuser_guid",
                table: "CharacterSheetReviewMessages",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetTags_charactersheet_id",
                table: "CharacterSheetTags",
                column: "charactersheet_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetTags_tag_guid",
                table: "CharacterSheetTags",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetVersion_charactersheet_id",
                table: "CharacterSheetVersion",
                column: "charactersheet_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetVersion_createdbyuser_guid",
                table: "CharacterSheetVersion",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetVersion_firstapprovalbyuser_guid",
                table: "CharacterSheetVersion",
                column: "firstapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetVersion_secondapprovalbyuser_guid",
                table: "CharacterSheetVersion",
                column: "secondapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetVersion_seriesguid",
                table: "CharacterSheetVersion",
                column: "seriesguid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheet_createdbyuser_guid",
                table: "ItemSheet",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheet_editby_user_guid",
                table: "ItemSheet",
                column: "editby_user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheet_firstapprovalbyuser_guid",
                table: "ItemSheet",
                column: "firstapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheet_itemtype_guid",
                table: "ItemSheet",
                column: "itemtype_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheet_secondapprovalbyuser_guid",
                table: "ItemSheet",
                column: "secondapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheet_seriesguid",
                table: "ItemSheet",
                column: "seriesguid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApproved_createdbyuser_guid",
                table: "ItemSheetApproved",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApproved_editby_user_guid",
                table: "ItemSheetApproved",
                column: "editby_user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApproved_firstapprovalbyuser_guid",
                table: "ItemSheetApproved",
                column: "firstapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApproved_itemtype_guid",
                table: "ItemSheetApproved",
                column: "itemtype_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApproved_secondapprovalbyuser_guid",
                table: "ItemSheetApproved",
                column: "secondapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApproved_seriesguid",
                table: "ItemSheetApproved",
                column: "seriesguid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApprovedTags_itemsheetapproved_id",
                table: "ItemSheetApprovedTags",
                column: "itemsheetapproved_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetApprovedTags_tag_guid",
                table: "ItemSheetApprovedTags",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetReviewMessages_createdbyuser_guid",
                table: "ItemSheetReviewMessages",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetTags_itemsheet_id",
                table: "ItemSheetTags",
                column: "itemsheet_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetTags_tag_guid",
                table: "ItemSheetTags",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetVersion_createdbyuser_guid",
                table: "ItemSheetVersion",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetVersion_firstapprovalbyuser_guid",
                table: "ItemSheetVersion",
                column: "firstapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetVersion_secondapprovalbyuser_guid",
                table: "ItemSheetVersion",
                column: "secondapprovalbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetVersion_seriesguid",
                table: "ItemSheetVersion",
                column: "seriesguid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUsers_Contacts_createdbyuser_guid",
                table: "ItemUsers_Contacts",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUsers_Contacts_user_guid",
                table: "ItemUsers_Contacts",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerCharacterSheetAllowed_createdbyuser_guid",
                table: "LARPPlayerCharacterSheetAllowed",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerCharacterSheetAllowed_larp_guid",
                table: "LARPPlayerCharacterSheetAllowed",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerCharacterSheetDisllowed_createdbyuser_guid",
                table: "LARPPlayerCharacterSheetDisllowed",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerCharacterSheetDisllowed_larp_guid",
                table: "LARPPlayerCharacterSheetDisllowed",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerSeriesAllowed_createdbyuser_guid",
                table: "LARPPlayerSeriesAllowed",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerSeriesAllowed_larp_guid",
                table: "LARPPlayerSeriesAllowed",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerSeriesAllowed_series_guid",
                table: "LARPPlayerSeriesAllowed",
                column: "series_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerSeriesDisllowed_createdbyuser_guid",
                table: "LARPPlayerSeriesDisllowed",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerSeriesDisllowed_larp_guid",
                table: "LARPPlayerSeriesDisllowed",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerSeriesDisllowed_series_guid",
                table: "LARPPlayerSeriesDisllowed",
                column: "series_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerTagAllowed_createdbyuser_guid",
                table: "LARPPlayerTagAllowed",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerTagAllowed_larp_guid",
                table: "LARPPlayerTagAllowed",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerTagAllowed_tag_guid",
                table: "LARPPlayerTagAllowed",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerTagDisllowed_createdbyuser_guid",
                table: "LARPPlayerTagDisllowed",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerTagDisllowed_larp_guid",
                table: "LARPPlayerTagDisllowed",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPPlayerTagDisllowed_tag_guid",
                table: "LARPPlayerTagDisllowed",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPRunPreReg_charactersheet_registered_approvedby_user",
                table: "LARPRunPreReg",
                column: "charactersheet_registered_approvedby_user");

            migrationBuilder.CreateIndex(
                name: "IX_LARPRunPreReg_larprun_guid",
                table: "LARPRunPreReg",
                column: "larprun_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPRunPreReg_user_guid",
                table: "LARPRunPreReg",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPRuns_createdbyuser_guid",
                table: "LARPRuns",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPRuns_larp_guid",
                table: "LARPRuns",
                column: "larp_guid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPTags_larpguid",
                table: "LARPTags",
                column: "larpguid");

            migrationBuilder.CreateIndex(
                name: "IX_LARPTags_tagguid",
                table: "LARPTags",
                column: "tagguid");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesTags_series_guid",
                table: "SeriesTags",
                column: "series_guid");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesTags_tag_guid",
                table: "SeriesTags",
                column: "tag_guid");

            migrationBuilder.CreateIndex(
                name: "IX_SheetUsers_Contacts_createdbyuser_guid",
                table: "SheetUsers_Contacts",
                column: "createdbyuser_guid");

            migrationBuilder.CreateIndex(
                name: "IX_SheetUsers_Contacts_user_guid",
                table: "SheetUsers_Contacts",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_approvedby_user_guid",
                table: "Tags",
                column: "approvedby_user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_tagtypeguid",
                table: "Tags",
                column: "tagtypeguid");

            migrationBuilder.CreateIndex(
                name: "IX_UserLARPRoles_larpguid",
                table: "UserLARPRoles",
                column: "larpguid");

            migrationBuilder.CreateIndex(
                name: "IX_UserLARPRoles_roleid",
                table: "UserLARPRoles",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "IX_UserLARPRoles_userguid",
                table: "UserLARPRoles",
                column: "userguid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_pronounsguid",
                table: "Users",
                column: "pronounsguid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterSheetApprovedTags");

            migrationBuilder.DropTable(
                name: "CharacterSheetReviewMessages");

            migrationBuilder.DropTable(
                name: "CharacterSheetTags");

            migrationBuilder.DropTable(
                name: "CharacterSheetVersion");

            migrationBuilder.DropTable(
                name: "ItemSheetApprovedTags");

            migrationBuilder.DropTable(
                name: "ItemSheetReviewMessages");

            migrationBuilder.DropTable(
                name: "ItemSheetTags");

            migrationBuilder.DropTable(
                name: "ItemSheetVersion");

            migrationBuilder.DropTable(
                name: "ItemUsers_Contacts");

            migrationBuilder.DropTable(
                name: "LARPPlayerCharacterSheetAllowed");

            migrationBuilder.DropTable(
                name: "LARPPlayerCharacterSheetDisllowed");

            migrationBuilder.DropTable(
                name: "LARPPlayerSeriesAllowed");

            migrationBuilder.DropTable(
                name: "LARPPlayerSeriesDisllowed");

            migrationBuilder.DropTable(
                name: "LARPPlayerTagAllowed");

            migrationBuilder.DropTable(
                name: "LARPPlayerTagDisllowed");

            migrationBuilder.DropTable(
                name: "LARPRunPreReg");

            migrationBuilder.DropTable(
                name: "LARPTags");

            migrationBuilder.DropTable(
                name: "SeriesTags");

            migrationBuilder.DropTable(
                name: "SheetUsers_Contacts");

            migrationBuilder.DropTable(
                name: "UserLARPRoles");

            migrationBuilder.DropTable(
                name: "CharacterSheetApproved");

            migrationBuilder.DropTable(
                name: "ItemSheetApproved");

            migrationBuilder.DropTable(
                name: "ItemSheet");

            migrationBuilder.DropTable(
                name: "LARPRuns");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "CharacterSheet");

            migrationBuilder.DropTable(
                name: "ItemTypes");

            migrationBuilder.DropTable(
                name: "LARPs");

            migrationBuilder.DropTable(
                name: "TagTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Pronouns");
        }
    }
}
