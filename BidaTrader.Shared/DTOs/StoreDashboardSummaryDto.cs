using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
    public class StoreDashboardSummaryDto
    {
        // Thông tin cửa hàng cơ bản
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }

        // 4 Ô Thống kê (Tháng hiện tại)
        public decimal CurrentMonthRevenue { get; set; }
        public int CurrentMonthOrders { get; set; }
        public int TotalActiveProducts { get; set; }
        public int TotalProductsSold { get; set; } // Tổng số lượng hàng bán ra
        public int TotalFollowers { get; set; }

        // So sánh với tháng trước (để hiện mũi tên tăng/giảm)
        public double RevenueGrowth { get; set; }
        public double OrderGrowth { get; set; }

        // Dữ liệu biểu đồ (12 tháng của năm nay)
        public List<MonthlyRevenueStatDto> RevenueChartData { get; set; } = new();
    }
}
