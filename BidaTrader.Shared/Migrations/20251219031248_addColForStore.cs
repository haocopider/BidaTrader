using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidaTrader.Shared.Migrations
{
    /// <inheritdoc />
    public partial class addColForStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Stores");
        }
    }
}
