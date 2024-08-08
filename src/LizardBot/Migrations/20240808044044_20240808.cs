using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LizardBot.Migrations
{
    /// <inheritdoc />
    public partial class _20240808 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.EnsureSchema(
                name: "lizardbot");

            migrationBuilder.RenameTable(
                name: "gpt_thread",
                newName: "gpt_thread",
                newSchema: "lizardbot");

            migrationBuilder.RenameTable(
                name: "bot_channel",
                newName: "bot_channel",
                newSchema: "lizardbot");

            migrationBuilder.CreateTable(
                name: "platform_user",
                schema: "lizardbot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_user", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_user",
                schema: "lizardbot");

            migrationBuilder.RenameTable(
                name: "gpt_thread",
                schema: "lizardbot",
                newName: "gpt_thread");

            migrationBuilder.RenameTable(
                name: "bot_channel",
                schema: "lizardbot",
                newName: "bot_channel");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });
        }
    }
}
