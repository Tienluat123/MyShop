using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Migrate data: Split FullName into FirstName and LastName
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET 
                    ""FirstName"" = CASE 
                        WHEN position(' ' in ""FullName"") > 0 THEN substring(""FullName"", 1, position(' ' in ""FullName"") - 1)
                        ELSE ""FullName""
                    END,
                    ""LastName"" = CASE 
                        WHEN position(' ' in ""FullName"") > 0 THEN ltrim(substring(""FullName"", position(' ' in ""FullName"") + 1))
                        ELSE ''
                    END;
            ");

            // Drop old FullName column
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back FullName column
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Restore FullName from FirstName + LastName
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""FullName"" = CASE 
                    WHEN ""FirstName"" = '' THEN ""LastName""
                    WHEN ""LastName"" = '' THEN ""FirstName""
                    ELSE ""FirstName"" || ' ' || ""LastName""
                END;
            ");

            // Drop new columns
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");
        }
    }
}
