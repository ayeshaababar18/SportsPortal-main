using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamIDToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamID",
                table: "tbl_Players",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Players_TeamID",
                table: "tbl_Players",
                column: "TeamID");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Players_tbl_Teams_TeamID",
                table: "tbl_Players",
                column: "TeamID",
                principalTable: "tbl_Teams",
                principalColumn: "TeamID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Players_tbl_Teams_TeamID",
                table: "tbl_Players");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Players_TeamID",
                table: "tbl_Players");

            migrationBuilder.DropColumn(
                name: "TeamID",
                table: "tbl_Players");
        }
    }
}
