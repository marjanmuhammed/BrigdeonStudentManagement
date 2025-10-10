using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgeon.Migrations
{
    /// <inheritdoc />
    public partial class FixedMentorRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MentorId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MentorId",
                table: "Users",
                column: "MentorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_MentorId",
                table: "Users",
                column: "MentorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_MentorId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MentorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MentorId",
                table: "Users");
        }
    }
}
