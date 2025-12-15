using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NEXUSDataLayerScaffold.Migrations
{
    /// <inheritdoc />
    public partial class NewStructureForShips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPRunPreReg",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdate",
                table: "LARPRunPreReg",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "larprunprereg_charactersheet_registered_approvedby_user_fkey",
                table: "LARPRunPreReg");

            migrationBuilder.DropForeignKey(
                name: "larprunprereg_larprun_guid_fkey",
                table: "LARPRunPreReg");

            migrationBuilder.DropForeignKey(
                name: "larprunprereg_user_guid_fkey",
                table: "LARPRunPreReg");

            migrationBuilder.DropForeignKey(
                name: "userlarproles_users_guid_fk",
                table: "UserLARPRoles");

            migrationBuilder.DropTable(
                name: "ShipCrewList");

            migrationBuilder.AlterColumn<bool>(
                name: "isactive",
                table: "LARPRunPreReg",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "createdate",
                table: "LARPRunPreReg",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_approvedby_user_guid",
                table: "Tags",
                column: "approvedby_user_guid");

            migrationBuilder.AddForeignKey(
                name: "LARPRunPreReg_charactersheet_registered_approvedby_user_fkey",
                table: "LARPRunPreReg",
                column: "charactersheet_registered_approvedby_user",
                principalTable: "Users",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "LARPRunPreReg_larprun_guid_fkey",
                table: "LARPRunPreReg",
                column: "larprun_guid",
                principalTable: "LARPRuns",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "LARPRunPreReg_user_guid_fkey",
                table: "LARPRunPreReg",
                column: "user_guid",
                principalTable: "Users",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "Tags_approvedby_fkey",
                table: "Tags",
                column: "approvedby_user_guid",
                principalTable: "Users",
                principalColumn: "guid");

            migrationBuilder.AddForeignKey(
                name: "UserLARPRoles_userguid_fkey",
                table: "UserLARPRoles",
                column: "userguid",
                principalTable: "Users",
                principalColumn: "guid");
        }
    }
}
