using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakWise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_DailyConsumption_DomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TotalKwh = table.Column<double>(type: "float", nullable: false),
                    TotalCost = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyConsumptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyConsumption_User_Date",
                table: "DailyConsumptions",
                columns: new[] { "UserId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyConsumptions");
        }
    }
}
