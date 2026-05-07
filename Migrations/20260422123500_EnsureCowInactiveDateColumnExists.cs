using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMFS.Migrations
{
    public partial class EnsureCowInactiveDateColumnExists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Cows', 'InactiveDate') IS NULL
BEGIN
    ALTER TABLE [dbo].[Cows] ADD [InactiveDate] datetime2 NULL;
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Cows', 'InactiveDate') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Cows] DROP COLUMN [InactiveDate];
END");
        }
    }
}
