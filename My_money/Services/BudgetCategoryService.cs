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
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly IRecordService _recordService;

        private static readonly BudgetCategory defaultBudgetCategory = new BudgetCategory() { Name = "Other" };

        public BudgetCategoryService(IBudgetCategoryRepository budgetCategoryRepository, IRecordService recordService)
        {
            _budgetCategoryRepository = budgetCategoryRepository;
            _recordService = recordService;
        }

        // TODO: Add validation and error handling as needed

        /// <summary>
        /// Gets all budget categories and enriches them with financial data for a specified period,
        /// including total spend, planned budget for the period, and remaining balance.
        /// </summary>
        /// <param name="from">The start date of the period (inclusive).</param>
        /// <param name="to">The end date of the period (inclusive).</param>
        /// <returns>
        /// A list of <see cref="BudgetCategory"/> objects with calculated period-based metrics.
        /// </returns>
        public async Task<List<BudgetCategory>> GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to)
        {
            var categories = await GetAllBudgetCategoriesAsync();
            var recordsByPeriod = await _recordService.GetRecordsByPeriodAsync(from, to);

            int periodLengthInDays = (to.Date - from.Date).Days + 1;

            var spendByCategory = recordsByPeriod
                .GroupBy(r => r.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));

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
            var categories = await _budgetCategoryRepository.GetAllAsync();
            if (categories.Count == 0) await AddBudgetCategoryAsync(defaultBudgetCategory);

            return categories;
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id)
        {
            return await _budgetCategoryRepository.GetByIdAsync(id);
        }

        public async Task<int> AddBudgetCategoryAsync(BudgetCategory category)
        {
            return await _budgetCategoryRepository.AddAsync(category);
        }

        public async Task UpdateBudgetCategoryAsync(BudgetCategory category)
        {
            var originalCategory = await _budgetCategoryRepository.GetByIdAsync(category.Id);

            if (originalCategory.Name == "Other")
            {
                throw new InvalidOperationException("You cannot change the 'Other' record type as it is a universal type!");
            }

            await _budgetCategoryRepository.UpdateAsync(category);
        }

        /// <summary>
        /// Deletes the specified budget category. 
        /// All records associated with this category are reassigned to the default "Other" category before deletion.
        /// </summary>
        /// <param name="category">The budget category to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to delete the default "Other" category.</exception>
        public async Task DeleteBudgetCategoryAsync(BudgetCategory category)
        {
            if (category.Name == "Other")
            {
                throw new InvalidOperationException("You cannot delete the 'Other' record type as it is a universal type!");
            }

            var defaultCategory = await _budgetCategoryRepository.GetByNameAsync("Other");

            if (defaultCategory is null)
            {
                throw new InvalidOperationException("Default category 'Other' was not found.");
            }

            var records = await _recordService.GetRecordsByCategoryIdAsync(category.Id);

            foreach (Record record in records)
            {
                record.CategoryId = defaultCategory.Id;
            }

            await _budgetCategoryRepository.DeleteAsync(category.Id);
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name)
        {
            return await _budgetCategoryRepository.GetByNameAsync(name);
        }

    }
}
