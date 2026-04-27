using My_money.Enums;
using My_money.Model;
using System;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IExplainableAnalyticsService
    {
        Task<DashboardAnalyticsSnapshot> BuildDashboardAnalyticsAsync(
            DateTime periodFrom,
            DateTime periodTo,
            DateTime trendAnchorDate,
            CategoryFilterType categoryFilterType,
            bool usesHouseholdFinance,
            int? householdId);
    }
}
