using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsPortal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithNewSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_AdminUsers",
                columns: table => new
                {
                    AdminID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_AdminUsers", x => x.AdminID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Announcements",
                columns: table => new
                {
                    AnnouncementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Priority = table.Column<string>(type: "varchar(20)", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Announcements", x => x.AnnouncementID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Departments",
                columns: table => new
                {
                    DeptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeptName = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    TotalPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Departments", x => x.DeptID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Seasons",
                columns: table => new
                {
                    SeasonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Seasons", x => x.SeasonID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Sports",
                columns: table => new
                {
                    SportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SportName = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Sports", x => x.SportID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Matches",
                columns: table => new
                {
                    MatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SportID = table.Column<int>(type: "int", nullable: false),
                    DeptA_ID = table.Column<int>(type: "int", nullable: true),
                    DeptB_ID = table.Column<int>(type: "int", nullable: true),
                    ScoreA = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    ScoreB = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SeasonID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Matches", x => x.MatchID);
                    table.ForeignKey(
                        name: "FK_tbl_Matches_tbl_Departments_DeptA_ID",
                        column: x => x.DeptA_ID,
                        principalTable: "tbl_Departments",
                        principalColumn: "DeptID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_Matches_tbl_Departments_DeptB_ID",
                        column: x => x.DeptB_ID,
                        principalTable: "tbl_Departments",
                        principalColumn: "DeptID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_Matches_tbl_Seasons_SeasonID",
                        column: x => x.SeasonID,
                        principalTable: "tbl_Seasons",
                        principalColumn: "SeasonID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Matches_tbl_Sports_SportID",
                        column: x => x.SportID,
                        principalTable: "tbl_Sports",
                        principalColumn: "SportID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Players",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    RegNumber = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DeptID = table.Column<int>(type: "int", nullable: false),
                    SportID = table.Column<int>(type: "int", nullable: false),
                    IsCaptain = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Players", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_tbl_Players_tbl_Departments_DeptID",
                        column: x => x.DeptID,
                        principalTable: "tbl_Departments",
                        principalColumn: "DeptID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Players_tbl_Sports_SportID",
                        column: x => x.SportID,
                        principalTable: "tbl_Sports",
                        principalColumn: "SportID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Teams",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SportID = table.Column<int>(type: "int", nullable: false),
                    DeptID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Teams", x => x.TeamID);
                    table.ForeignKey(
                        name: "FK_tbl_Teams_tbl_Departments_DeptID",
                        column: x => x.DeptID,
                        principalTable: "tbl_Departments",
                        principalColumn: "DeptID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Teams_tbl_Sports_SportID",
                        column: x => x.SportID,
                        principalTable: "tbl_Sports",
                        principalColumn: "SportID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Matches_DeptA_ID",
                table: "tbl_Matches",
                column: "DeptA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Matches_DeptB_ID",
                table: "tbl_Matches",
                column: "DeptB_ID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Matches_SeasonID",
                table: "tbl_Matches",
                column: "SeasonID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Matches_SportID",
                table: "tbl_Matches",
                column: "SportID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Players_DeptID",
                table: "tbl_Players",
                column: "DeptID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Players_SportID",
                table: "tbl_Players",
                column: "SportID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Teams_DeptID",
                table: "tbl_Teams",
                column: "DeptID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Teams_SportID",
                table: "tbl_Teams",
                column: "SportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_AdminUsers");

            migrationBuilder.DropTable(
                name: "tbl_Announcements");

            migrationBuilder.DropTable(
                name: "tbl_Matches");

            migrationBuilder.DropTable(
                name: "tbl_Players");

            migrationBuilder.DropTable(
                name: "tbl_Teams");

            migrationBuilder.DropTable(
                name: "tbl_Seasons");

            migrationBuilder.DropTable(
                name: "tbl_Departments");

            migrationBuilder.DropTable(
                name: "tbl_Sports");
        }
    }
}
