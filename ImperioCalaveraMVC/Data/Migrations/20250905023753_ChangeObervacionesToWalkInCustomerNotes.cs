using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImperioCalaveraMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeObervacionesToWalkInCustomerNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "Citas",
                newName: "WalkInCustomerNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WalkInCustomerNotes",
                table: "Citas",
                newName: "Observaciones");
        }
    }
}
