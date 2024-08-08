using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LizardBot.Migrations
{
    /// <inheritdoc />
    public partial class _20240808v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenUsage",
                schema: "lizardbot",
                table: "gpt_thread",
                newName: "OutputUsage");

            migrationBuilder.AddColumn<int>(
                name: "InputUsage",
                schema: "lizardbot",
                table: "gpt_thread",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "NoticeId",
                schema: "lizardbot",
                table: "bot_channel",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputUsage",
                schema: "lizardbot",
                table: "gpt_thread");

            migrationBuilder.RenameColumn(
                name: "OutputUsage",
                schema: "lizardbot",
                table: "gpt_thread",
                newName: "TokenUsage");

            migrationBuilder.AlterColumn<decimal>(
                name: "NoticeId",
                schema: "lizardbot",
                table: "bot_channel",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }
    }
}
