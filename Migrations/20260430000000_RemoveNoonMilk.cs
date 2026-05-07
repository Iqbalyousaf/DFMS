using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMFS.Migrations
{
    public partial class RemoveNoonMilk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // drop NoonMilk column from MilkProductions if it exists
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NoonMilk' AND Object_ID = Object_ID(N'dbo.MilkProductions'))
BEGIN
    ALTER TABLE dbo.MilkProductions DROP COLUMN NoonMilk
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restore NoonMilk column if rolling back
            migrationBuilder.AddColumn<double>(
                name: "NoonMilk",
                table: "MilkProductions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
