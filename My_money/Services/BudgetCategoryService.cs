using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class BudgetCategoryService : IBudgetCategoryService
    {
        private readonly IBudgetCategoryService _budgetCategoryService;

        public BudgetCategoryService(IBudgetCategoryService budgetCategoryService)
        {
            _budgetCategoryService = budgetCategoryService;
        }

        //TODO: Add validation and error handling as needed

        public async Task<List<BudgetCategory>> GetAllBudgetCategoriesAsync()
        {
            return await _budgetCategoryService.GetAllBudgetCategoriesAsync();
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id)
        {
            return await _budgetCategoryService.GetBudgetCategoryByIdAsync(id);
        }

        public async Task<int> AddBudgetCategoryAsync(BudgetCategory category)
        {
            return await _budgetCategoryService.AddBudgetCategoryAsync(category);
        }

        public async Task UpdateBudgetCategoryAsync(BudgetCategory category)
        {
            await _budgetCategoryService.UpdateBudgetCategoryAsync(category);
        }

        public async Task DeleteBudgetCategoryAsync(int id)
        {
            await _budgetCategoryService.DeleteBudgetCategoryAsync(id);
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name)
        {
            return await _budgetCategoryService.GetBudgetCategoryByNameAsync(name);
        }
    }
}
