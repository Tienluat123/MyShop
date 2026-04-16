using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkScheduleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "WorkSchedules");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "WorkSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WorkSchedules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEditing",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPasswordVisible",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "IsEditing",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPasswordVisible",
                table: "Users");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "WorkSchedules",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "WorkSchedules",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}
