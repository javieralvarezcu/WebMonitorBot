using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMonitorBot.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class FixWhitelistIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Changing IDENTITY property requires recreating the table. Perform safe copy to a temp table.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.Whitelist_temp', N'U') IS NOT NULL
                    DROP TABLE dbo.Whitelist_temp;

                SELECT ChatId INTO dbo.Whitelist_temp FROM dbo.Whitelist;

                DROP TABLE dbo.Whitelist;

                CREATE TABLE dbo.Whitelist (
                    ChatId bigint NOT NULL PRIMARY KEY
                );

                INSERT INTO dbo.Whitelist (ChatId) SELECT ChatId FROM dbo.Whitelist_temp;

                DROP TABLE dbo.Whitelist_temp;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate table with IDENTITY as it was before.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.Whitelist_temp', N'U') IS NOT NULL
                    DROP TABLE dbo.Whitelist_temp;

                SELECT ChatId INTO dbo.Whitelist_temp FROM dbo.Whitelist;

                DROP TABLE dbo.Whitelist;

                CREATE TABLE dbo.Whitelist (
                    ChatId bigint NOT NULL IDENTITY(1,1) PRIMARY KEY
                );

                SET IDENTITY_INSERT dbo.Whitelist ON;
                INSERT INTO dbo.Whitelist (ChatId) SELECT ChatId FROM dbo.Whitelist_temp;
                SET IDENTITY_INSERT dbo.Whitelist OFF;

                DROP TABLE dbo.Whitelist_temp;
            ");
        }
    }
}
