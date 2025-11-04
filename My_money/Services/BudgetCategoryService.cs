using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class BudgetCategoryService : IBudgetCategoryService
    {
        private readonly IBudgetCategoryRepository _budgetCategoryService;
        private readonly IRecordService _recordService;

        public BudgetCategoryService(IBudgetCategoryRepository budgetCategoryRepository, IRecordService recordService)
        {
            _budgetCategoryService = budgetCategoryRepository;
            _recordService = recordService;
        }

        //TODO: Add validation and error handling as needed


        public async Task<List<BudgetCategory>> GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to)
        {
            var categories = await _budgetCategoryService.GetAllAsync();
            var recordsByPeriod = await _recordService.GetRecordsByPeriodAsync(from, to);

            int periodLengthInDays = (to.Date - from.Date).Days + 1;

            var spendByCategory = recordsByPeriod
                .GroupBy(r => r.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Cost));

            foreach (var category in categories)
            {
                decimal totalSpend = spendByCategory.TryGetValue(category.Id, out var spend) ? spend : 0;

                decimal monthlyPlan = category.Plan ?? 0;
                decimal dailyPlan = monthlyPlan / 30m;
                decimal planForPeriod = dailyPlan * periodLengthInDays;

                category.SpendByPeriod = totalSpend;
                category.PlanByPeriod = planForPeriod;
                category.RemainingByPeriod = planForPeriod - totalSpend;
            }

            return categories;
        }

        public async Task<List<BudgetCategory>> GetAllBudgetCategoriesAsync()
        {
            return await _budgetCategoryService.GetAllAsync();
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id)
        {
            return await _budgetCategoryService.GetByIdAsync(id);
        }

        public async Task<int> AddBudgetCategoryAsync(BudgetCategory category)
        {
            return await _budgetCategoryService.AddAsync(category);
        }

        public async Task UpdateBudgetCategoryAsync(BudgetCategory category)
        {
            await _budgetCategoryService.UpdateAsync(category);
        }

        public async Task DeleteBudgetCategoryAsync(int id)
        {
            await _budgetCategoryService.DeleteAsync(id);
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name)
        {
            return await _budgetCategoryService.GetByNameAsync(name);
        }
    }
}
