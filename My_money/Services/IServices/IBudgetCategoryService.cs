using My_money.Enums;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IBudgetCategoryService
    {
        /// <summary>
        /// Get list for household member type Child in personal finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="BudgetCategory"/> objects created by the authenticated user within the specified household.</returns>
        Task<List<BudgetCategory>> GetAllByHouseholdAndCreatedByAsync(int householdId);
        /// <summary>
        /// Get list for Household in shared finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="BudgetCategory"/> objects associated with the specified household.</returns>
        Task<List<BudgetCategory>> GetAllByHouseholdIdAsync(int householdId);
        /// <summary>
        /// Get list in personal finances
        /// </summary>
        /// <returns>A list of <see cref="BudgetCategory"/> objects associated with the authenticated user.</returns>
        Task<List<BudgetCategory>> GetAllByOwnerAsync();

        Task<List<BudgetCategory>> GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to, CategoryFilterType categoryFilterType, int? householdId);
        Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id);
        Task<int> AddBudgetCategoryAsync(BudgetCategory category);
        Task UpdateBudgetCategoryAsync(BudgetCategory category);
        Task DeleteBudgetCategoryAsync(BudgetCategory category);
        Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name, int householdId, int? ownerUserId, string scope);
    }
}
