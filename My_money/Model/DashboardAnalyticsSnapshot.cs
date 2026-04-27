using System.Collections.Generic;

namespace My_money.Model
{
    public class DashboardAnalyticsSnapshot
    {
        public string OverviewText { get; set; } = string.Empty;
        public List<AnalyticsMetric> Metrics { get; set; } = [];
        public List<AnalyticsInsight> Insights { get; set; } = [];
        public List<MonthlySpendTrendPoint> MonthlyTrend { get; set; } = [];
    }
}
