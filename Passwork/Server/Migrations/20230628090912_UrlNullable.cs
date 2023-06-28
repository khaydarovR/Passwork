using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Passwork.Server.Migrations
{
    public partial class UrlNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UseInUtl",
                table: "Passwords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 6, 28, 9, 9, 12, 439, DateTimeKind.Utc).AddTicks(7315),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 6, 27, 11, 23, 26, 192, DateTimeKind.Utc).AddTicks(3077));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UseInUtl",
                table: "Passwords",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 6, 27, 11, 23, 26, 192, DateTimeKind.Utc).AddTicks(3077),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 6, 28, 9, 9, 12, 439, DateTimeKind.Utc).AddTicks(7315));
        }
    }
}
