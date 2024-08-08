using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LizardBot.Migrations
{
    /// <inheritdoc />
    public partial class _20240808v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NoticeId",
                schema: "lizardbot",
                table: "bot_channel",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoticeId",
                schema: "lizardbot",
                table: "bot_channel");
        }
    }
}
