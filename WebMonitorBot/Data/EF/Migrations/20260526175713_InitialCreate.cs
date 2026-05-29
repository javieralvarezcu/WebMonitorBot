using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitorBot.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoringUrls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastCleanText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckIntervalSeconds = table.Column<int>(type: "int", nullable: true),
                    LastCheckedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringUrls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Whitelist",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Whitelist", x => x.ChatId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringUrls");

            migrationBuilder.DropTable(
                name: "Whitelist");
        }
    }
}
