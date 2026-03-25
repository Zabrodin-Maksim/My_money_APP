using My_money.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IBudgetCategoryService
    {
        Task<List<BudgetCategory>> GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to);
        Task<List<BudgetCategory>> GetAllBudgetCategoriesAsync();
        Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id);
        Task<int> AddBudgetCategoryAsync(BudgetCategory category);
        Task UpdateBudgetCategoryAsync(BudgetCategory category);
        Task DeleteBudgetCategoryAsync(BudgetCategory category);
        Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name);
    }
}
