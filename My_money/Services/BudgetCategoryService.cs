using My_money.Constants;
using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
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
        private readonly IUserSessionService _userSessionService;

        public BudgetCategoryService(IBudgetCategoryRepository budgetCategoryRepository, IRecordService recordService, IUserSessionService userSessionService)
        {
            _budgetCategoryRepository = budgetCategoryRepository;
            _recordService = recordService;
            _userSessionService = userSessionService;
        }

        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            return _userSessionService.CurrentUser!.Id;
        }

        /// <summary>
        /// Retrieves budget categories based on the specified filter
        /// and enriches them with financial data for the given period.
        /// </summary>
        /// <param name="from">The start date of the period (inclusive).</param>
        /// <param name="to">The end date of the period (inclusive).</param>
        /// <param name="categoryFilterType">The category source/filter to apply.</param>
        /// <param name="householdId">
        /// The household ID required for household-related filters.
        /// </param>
        /// <returns>
        /// A list of <see cref="BudgetCategory"/> objects with calculated spending,
        /// planned amount, and remaining balance for the specified period.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="householdId"/> is required but not provided.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when an invalid <paramref name="categoryFilterType"/> is specified.
        /// </exception>
        public async Task<List<BudgetCategory>> GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to, CategoryFilterType categoryFilterType, int? householdId)
        {
            List<BudgetCategory> categories = categoryFilterType switch
            {
                CategoryFilterType.Household => householdId.HasValue
                    ? await GetAllByHouseholdIdAsync(householdId.Value)
                    : throw new ArgumentNullException(nameof(householdId)),

                CategoryFilterType.Personal => await GetAllByOwnerAsync(),

                CategoryFilterType.Child => householdId.HasValue
                    ? await GetAllByHouseholdAndCreatedByAsync(householdId.Value)
                    : throw new ArgumentNullException(nameof(householdId)),

                _ => throw new ArgumentException($"Invalid source: {categoryFilterType}", nameof(categoryFilterType))
            };

            var recordsByPeriod = await _recordService.GetRecordsByPeriodAsync(from, to, householdId);

            int periodLengthInDays = (to.Date - from.Date).Days + 1;

            var spendByCategory = recordsByPeriod.Where(r => r.Type == RecordConstants.Types.Expense && r.CategoryId.HasValue)
                .GroupBy(r => r.CategoryId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));

            foreach (var category in categories)
            {
                decimal totalSpend = spendByCategory.TryGetValue(category.Id, out var spend) ? spend : 0;

                decimal monthlyPlan = category.Plan;
                decimal dailyPlan = monthlyPlan / 30m;
                decimal planForPeriod = dailyPlan * periodLengthInDays;

                category.SpendByPeriod = totalSpend;
                category.PlanByPeriod = planForPeriod;
                category.RemainingByPeriod = planForPeriod - totalSpend;
            }

            return categories;
        }

        /// <summary>
        /// Get list for household member type Child in personal finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="BudgetCategory"/> objects created by the authenticated user within the specified household.</returns>
        public async Task<List<BudgetCategory>> GetAllByHouseholdAndCreatedByAsync(int householdId)
        {
            return await _budgetCategoryRepository.GetAllByHouseholdAndCreatedByAsync(householdId, GetAuthenticatedUserId());
        }

        /// <summary>
        /// Get list for Household in shared finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="BudgetCategory"/> objects associated with the specified household.</returns>
        public async Task<List<BudgetCategory>> GetAllByHouseholdIdAsync(int householdId)
        {
            return await _budgetCategoryRepository.GetAllByHouseholdIdAsync(householdId);
        }

        /// <summary>
        /// Get list in personal finances
        /// </summary>
        /// <returns>A list of <see cref="BudgetCategory"/> objects associated with the authenticated user.</returns>
        public async Task<List<BudgetCategory>> GetAllByOwnerAsync()
        {
            var userId = GetAuthenticatedUserId();
            return await _budgetCategoryRepository.GetAllByOwnerAsync(userId);
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id)
        {
            return await _budgetCategoryRepository.GetByIdAsync(id);
        }

        public async Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name, int householdId, int? ownerUserId, string scope)
        {
            return await _budgetCategoryRepository.GetByNameAsync(name, householdId, ownerUserId, scope);
        }

        public async Task UpdateBudgetCategoryAsync(BudgetCategory category)
        {
            var originalCategory = await _budgetCategoryRepository.GetByIdAsync(category.Id);

            if (originalCategory!.Name == "Other")
            {
                throw new InvalidOperationException("You cannot change the 'Other' record type as it is a universal type!");
            }

            await _budgetCategoryRepository.UpdateAsync(category);
        }

        public async Task<int> AddBudgetCategoryAsync(BudgetCategory category)
        {
            return await _budgetCategoryRepository.AddAsync(category);
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

            var defaultCategory = await _budgetCategoryRepository.GetByNameAsync("Other", category.HouseholdId, category.OwnerUserId, category.Scope);

            if (defaultCategory is null)
            {
                throw new InvalidOperationException("Default category 'Other' was not found.");
            }

            var records = await _recordService.GetRecordsByCategoryIdAsync(category.Id);

            foreach (Record record in records)
            {
                record.CategoryId = defaultCategory.Id;
                await _recordService.UpdateRecordAsync(record);
            }

            await _budgetCategoryRepository.DeleteAsync(category.Id);
        }
    }
}
