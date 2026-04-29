using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamCategoryAndGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "tbl_Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "tbl_Players");
        }
    }
}
