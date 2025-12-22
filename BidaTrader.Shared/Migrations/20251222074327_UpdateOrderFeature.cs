using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidaTrader.Shared.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===============================
            // 1. ĐỔI TÊN CỘT (GIỮ DỮ LIỆU)
            // ===============================
            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "Orders",
                newName: "IsPaid");

            // ===============================
            // 2. ALTER CỘT CŨ
            // ===============================
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // ===============================
            // 3. THÊM CỘT MỚI
            // ===============================
            migrationBuilder.AddColumn<DateTime>("ConfirmedAt", "Orders", nullable: true);
            migrationBuilder.AddColumn<DateTime>("ShippedAt", "Orders", nullable: true);
            migrationBuilder.AddColumn<DateTime>("CompletedAt", "Orders", nullable: true);
            migrationBuilder.AddColumn<DateTime>("CancelledAt", "Orders", nullable: true);

            migrationBuilder.AddColumn<DateTime>("PaidAt", "Orders", nullable: true);
            migrationBuilder.AddColumn<string>("ShippingProvider", "Orders", nullable: true);
            migrationBuilder.AddColumn<string>("TrackingCode", "Orders", nullable: true);
            migrationBuilder.AddColumn<string>("Note", "Orders", nullable: true);

            migrationBuilder.AddColumn<int>("UpdatedBy", "Orders", nullable: true);
            migrationBuilder.AddColumn<DateTime>("UpdatedAt", "Orders", nullable: true);

            // ===============================
            // 4. SNAPSHOT PRODUCT NAME
            // ===============================
            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa các cột mới
            migrationBuilder.DropColumn("ConfirmedAt", "Orders");
            migrationBuilder.DropColumn("ShippedAt", "Orders");
            migrationBuilder.DropColumn("CompletedAt", "Orders");
            migrationBuilder.DropColumn("CancelledAt", "Orders");
            migrationBuilder.DropColumn("PaidAt", "Orders");
            migrationBuilder.DropColumn("ShippingProvider", "Orders");
            migrationBuilder.DropColumn("TrackingCode", "Orders");
            migrationBuilder.DropColumn("Note", "Orders");
            migrationBuilder.DropColumn("UpdatedBy", "Orders");
            migrationBuilder.DropColumn("UpdatedAt", "Orders");

            migrationBuilder.DropColumn("ProductName", "OrderDetails");

            // Trả lại kiểu cũ
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Đổi tên lại IsPaid → PaymentStatus
            migrationBuilder.RenameColumn(
                name: "IsPaid",
                table: "Orders",
                newName: "PaymentStatus");
        }
    }
}
