using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidaTrader.Shared.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "Accounts",
                type: "varbinary(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "Accounts",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Accounts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Accounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(500)",
                oldMaxLength: 500);
        }
    }
}
