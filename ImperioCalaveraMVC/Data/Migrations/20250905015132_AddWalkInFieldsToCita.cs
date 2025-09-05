using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImperioCalaveraMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWalkInFieldsToCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalkInCustomerName",
                table: "Citas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalkInCustomerPhone",
                table: "Citas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalkInCustomerName",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "WalkInCustomerPhone",
                table: "Citas");
        }
    }
}
