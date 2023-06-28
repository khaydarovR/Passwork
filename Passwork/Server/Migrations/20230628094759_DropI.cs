using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwork.Server.Migrations
{
    public partial class DropI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "PasswordTags");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 6, 28, 9, 47, 59, 44, DateTimeKind.Utc).AddTicks(81),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 6, 28, 9, 9, 12, 439, DateTimeKind.Utc).AddTicks(7315));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PasswordTags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 6, 28, 9, 9, 12, 439, DateTimeKind.Utc).AddTicks(7315),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 6, 28, 9, 47, 59, 44, DateTimeKind.Utc).AddTicks(81));
        }
    }
}
