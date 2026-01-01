using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
        public class MonthlyRevenueStatDto
        {
            public int Month { get; set; }
            public string MonthLabel => $"T{Month}";
            public decimal Revenue { get; set; }
            public int OrderCount { get; set; }
        }

        public class YearlyRevenueStatDto
        {
            public int Year { get; set; }
            public string YearLabel => $"Năm {Year}";
            public decimal Revenue { get; set; }
            public int OrderCount { get; set; }
        }

        public class StoreRevenueStatsResponse
        {
            public List<int> AvailableYears { get; set; } = new();
            public List<MonthlyRevenueStatDto> MonthlyStats { get; set; } = new();
            public List<YearlyRevenueStatDto> YearlyStats { get; set; } = new();
        }
    
}
