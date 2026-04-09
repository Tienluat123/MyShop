using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Migrations
{
    /// <inheritdoc />
    public partial class SplitCustomerPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Customers",
                newName: "TotalAccumulatedPoints");

            migrationBuilder.AddColumn<int>(
                name: "CurrentAvailablePoints",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAvailablePoints",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "TotalAccumulatedPoints",
                table: "Customers",
                newName: "Points");
        }
    }
}
