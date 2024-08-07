using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LizardBot.Migrations
{
    /// <inheritdoc />
    public partial class _20240807_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bot_channel",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SettingType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_channel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gpt_thread",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssistantId = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenUsage = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gpt_thread", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_channel");

            migrationBuilder.DropTable(
                name: "gpt_thread");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
