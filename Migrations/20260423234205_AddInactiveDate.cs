using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMFS.Migrations
{
    /// <inheritdoc />
    public partial class AddInactiveDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InactiveDate",
                table: "Cows",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InactiveDate",
                table: "Cows");
        }
    }
}
