using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMFS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cows",
                columns: table => new
                {
                    CowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CowName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EarTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Breed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WeightAtBirth = table.Column<double>(type: "float", nullable: false),
                    Pasture = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cows", x => x.CowId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpDob = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpPass = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmpId);
                });

            migrationBuilder.CreateTable(
                name: "BreedReports",
                columns: table => new
                {
                    Brid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeatDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CowId = table.Column<int>(type: "int", nullable: false),
                    CowName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PregDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCalved = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CowAge = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedReports", x => x.Brid);
                    table.ForeignKey(
                        name: "FK_BreedReports_Cows_CowId",
                        column: x => x.CowId,
                        principalTable: "Cows",
                        principalColumn: "CowId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthReports",
                columns: table => new
                {
                    RepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CowId = table.Column<int>(type: "int", nullable: false),
                    CowName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Treatment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VetName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthReports", x => x.RepId);
                    table.ForeignKey(
                        name: "FK_HealthReports_Cows_CowId",
                        column: x => x.CowId,
                        principalTable: "Cows",
                        principalColumn: "CowId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilkProductions",
                columns: table => new
                {
                    MId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CowId = table.Column<int>(type: "int", nullable: false),
                    CowName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateProd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmMilk = table.Column<double>(type: "float", nullable: false),
                    NoonMilk = table.Column<double>(type: "float", nullable: false),
                    PmMilk = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkProductions", x => x.MId);
                    table.ForeignKey(
                        name: "FK_MilkProductions_Cows_CowId",
                        column: x => x.CowId,
                        principalTable: "Cows",
                        principalColumn: "CowId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenditureEntries",
                columns: table => new
                {
                    ExpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenditureEntries", x => x.ExpId);
                    table.ForeignKey(
                        name: "FK_ExpenditureEntries_Employees_EmpId",
                        column: x => x.EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncomeEntries",
                columns: table => new
                {
                    IncId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncAmt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeEntries", x => x.IncId);
                    table.ForeignKey(
                        name: "FK_IncomeEntries_Employees_EmpId",
                        column: x => x.EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilkSales",
                columns: table => new
                {
                    SId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Uprice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmpId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkSales", x => x.SId);
                    table.ForeignKey(
                        name: "FK_MilkSales_Employees_EmpId",
                        column: x => x.EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BreedReports_CowId",
                table: "BreedReports",
                column: "CowId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenditureEntries_EmpId",
                table: "ExpenditureEntries",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthReports_CowId",
                table: "HealthReports",
                column: "CowId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeEntries_EmpId",
                table: "IncomeEntries",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkProductions_CowId",
                table: "MilkProductions",
                column: "CowId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkSales_EmpId",
                table: "MilkSales",
                column: "EmpId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreedReports");

            migrationBuilder.DropTable(
                name: "ExpenditureEntries");

            migrationBuilder.DropTable(
                name: "HealthReports");

            migrationBuilder.DropTable(
                name: "IncomeEntries");

            migrationBuilder.DropTable(
                name: "MilkProductions");

            migrationBuilder.DropTable(
                name: "MilkSales");

            migrationBuilder.DropTable(
                name: "Cows");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
