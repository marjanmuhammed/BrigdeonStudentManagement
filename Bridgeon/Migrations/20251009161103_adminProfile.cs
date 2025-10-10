using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgeon.Migrations
{
    /// <inheritdoc />
    public partial class adminProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentorAssignments");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Profiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Profiles");

            migrationBuilder.CreateTable(
                name: "MentorAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenteeId = table.Column<int>(type: "int", nullable: false),
                    MentorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorAssignments_Users_MenteeId",
                        column: x => x.MenteeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorAssignments_Users_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentorAssignments_MenteeId",
                table: "MentorAssignments",
                column: "MenteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorAssignments_MentorId",
                table: "MentorAssignments",
                column: "MentorId");
        }
    }
}
