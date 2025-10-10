using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgeon.Migrations
{
    /// <inheritdoc />
    public partial class adminprofileupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "PortfolioUrl",
                table: "Profiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "Profiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "Profiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioUrl",
                table: "Profiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
