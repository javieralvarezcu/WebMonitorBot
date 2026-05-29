using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitorBot.Data.EF.Migrations
{
    [DbContext(typeof(WebMonitorContext))]
    [Migration("20260529120000_RecreateMonitoring")]
    public partial class RecreateMonitoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old table if it exists (safe when data loss is acceptable)
            migrationBuilder.Sql("IF OBJECT_ID('dbo.MonitoreoUrls', 'U') IS NOT NULL DROP TABLE dbo.MonitoreoUrls;");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringUrls");

            migrationBuilder.CreateTable(
                name: "MonitoreoUrls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UltimoTextoLimpio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UltimoHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckIntervalSeconds = table.Column<int>(type: "int", nullable: true),
                    LastCheckedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoreoUrls", x => x.Id);
                });
        }
    }
}
