using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NEXUSDataLayerScaffold.Migrations
{
    /// <inheritdoc />
    public partial class QolUpdates202502 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "UserLARPRoles",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isapproved",
                table: "Tags",
                type: "boolean",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "false");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "Tags",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "Series",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPTags",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPs",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPRuns",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPRunPreReg",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerTagDisllowed",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerTagAllowed",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerSeriesDisllowed",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerSeriesAllowed",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerCharacterSheetDisllowed",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerCharacterSheetAllowed",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemTypes",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "ItemSheetVersion",
                type: "integer",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValueSql: "1");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheetVersion",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheetReviewMessages",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "ItemSheetApproved",
                type: "integer",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValueSql: "1");

            migrationBuilder.AlterColumn<bool>(
                name: "isdoubleside",
                table: "ItemSheetApproved",
                type: "boolean",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "false");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheetApproved",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "ItemSheet",
                type: "integer",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValueSql: "1");

            migrationBuilder.AlterColumn<bool>(
                name: "readyforapproval",
                table: "ItemSheet",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "isdoubleside",
                table: "ItemSheet",
                type: "boolean",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "false");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheet",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "CharacterSheetVersion",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "1");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheetVersion",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheetReviewMessages",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "CharacterSheetApproved",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "1");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheetApproved",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "CharacterSheet",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "1");

            migrationBuilder.AlterColumn<bool>(
                name: "readyforapproval",
                table: "CharacterSheet",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheet",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.CreateTable(
                name: "CharacterSheetMessageAcks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    charactersheetreviewmessages_id = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    seendate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("charactersheetmessageacks_id", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetMessageAcks_CharacterSheetReviewMessages_id_fkey",
                        column: x => x.charactersheetreviewmessages_id,
                        principalTable: "CharacterSheetReviewMessages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "CharacterSheetMessageAcks_user_guid_fkey",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheetReviewSubscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    charactersheet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    stopdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("charactersheetreviewsubscriptions_id", x => x.id);
                    table.ForeignKey(
                        name: "CharacterSheetReviewSubscriptions_user_guid_fkey",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetMessageAcks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    itemsheetreviewmessages_id = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    seendate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemsheetmessageacks_id", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetMessageAcks_ItemSheetReviewMessages_id_fkey",
                        column: x => x.itemsheetreviewmessages_id,
                        principalTable: "ItemSheetReviewMessages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "ItemSheetMessageAcks_user_guid_fkey",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateTable(
                name: "ItemSheetReviewSubscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    itemsheet_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    stopdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("itemsheetreviewsubscriptions_id", x => x.id);
                    table.ForeignKey(
                        name: "ItemSheetReviewSubscriptions_user_guid_fkey",
                        column: x => x.user_guid,
                        principalTable: "Users",
                        principalColumn: "guid");
                });

            migrationBuilder.CreateIndex(
                name: "items_active",
                table: "ItemSheet",
                column: "isactive");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetMessageAcks_charactersheetreviewmessages_id",
                table: "CharacterSheetMessageAcks",
                column: "charactersheetreviewmessages_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetMessageAcks_user_guid",
                table: "CharacterSheetMessageAcks",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheetReviewSubscriptions_user_guid",
                table: "CharacterSheetReviewSubscriptions",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetMessageAcks_itemsheetreviewmessages_id",
                table: "ItemSheetMessageAcks",
                column: "itemsheetreviewmessages_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetMessageAcks_user_guid",
                table: "ItemSheetMessageAcks",
                column: "user_guid");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSheetReviewSubscriptions_user_guid",
                table: "ItemSheetReviewSubscriptions",
                column: "user_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterSheetMessageAcks");

            migrationBuilder.DropTable(
                name: "CharacterSheetReviewSubscriptions");

            migrationBuilder.DropTable(
                name: "ItemSheetMessageAcks");

            migrationBuilder.DropTable(
                name: "ItemSheetReviewSubscriptions");

            migrationBuilder.DropIndex(
                name: "items_active",
                table: "ItemSheet");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "UserLARPRoles",
                type: "boolean",
                nullable: true,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isapproved",
                table: "Tags",
                type: "boolean",
                nullable: true,
                defaultValueSql: "false",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "Tags",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "Series",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPTags",
                type: "boolean",
                nullable: true,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPs",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPRuns",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPRunPreReg",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerTagDisllowed",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerTagAllowed",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerSeriesDisllowed",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerSeriesAllowed",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerCharacterSheetDisllowed",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPPlayerCharacterSheetAllowed",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemTypes",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "ItemSheetVersion",
                type: "integer",
                nullable: true,
                defaultValueSql: "1",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheetVersion",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheetReviewMessages",
                type: "boolean",
                nullable: true,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "ItemSheetApproved",
                type: "integer",
                nullable: true,
                defaultValueSql: "1",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "isdoubleside",
                table: "ItemSheetApproved",
                type: "boolean",
                nullable: true,
                defaultValueSql: "false",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheetApproved",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "ItemSheet",
                type: "integer",
                nullable: true,
                defaultValueSql: "1",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "readyforapproval",
                table: "ItemSheet",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "isdoubleside",
                table: "ItemSheet",
                type: "boolean",
                nullable: true,
                defaultValueSql: "false",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "ItemSheet",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "CharacterSheetVersion",
                type: "integer",
                nullable: false,
                defaultValueSql: "1",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheetVersion",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheetReviewMessages",
                type: "boolean",
                nullable: true,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "CharacterSheetApproved",
                type: "integer",
                nullable: false,
                defaultValueSql: "1",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheetApproved",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "version",
                table: "CharacterSheet",
                type: "integer",
                nullable: false,
                defaultValueSql: "1",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "readyforapproval",
                table: "CharacterSheet",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "CharacterSheet",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
