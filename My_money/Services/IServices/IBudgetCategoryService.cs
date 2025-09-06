using My_money.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IBudgetCategoryService
    {
        Task<List<BudgetCategory>> GetAllBudgetCategoriesAsync();
        Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id);
        Task<int> AddBudgetCategoryAsync(BudgetCategory category); 
        Task UpdateBudgetCategoryAsync(BudgetCategory category);
        Task DeleteBudgetCategoryAsync(int id);
        Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name);
    }
}
