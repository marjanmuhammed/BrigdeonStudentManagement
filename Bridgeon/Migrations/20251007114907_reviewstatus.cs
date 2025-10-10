using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgeon.Migrations
{
    /// <inheritdoc />
    public partial class reviewstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeeCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PendingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FeeStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserReviews_UserId",
                table: "UserReviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserReviews");
        }
    }
}
