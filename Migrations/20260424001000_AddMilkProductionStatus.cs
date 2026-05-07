using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMFS.Migrations
{
    public partial class AddMilkProductionStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MilkProductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Draft");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "MilkProductions");
        }
    }
}
