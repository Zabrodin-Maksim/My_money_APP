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
    public class RecordService : IRecordService
    {
        private readonly IRecordRepository _recordRepository;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IHouseholdFinanceService _householdFinanceService;

        public RecordService(IRecordRepository recordRepository, IUserFinanceService userFinanceService, IBudgetCategoryRepository budgetCategoryRepository,
            IUserSessionService userSessionService, IHouseholdFinanceService householdFinanceService)
        {
            _recordRepository = recordRepository;
            _userFinanceService = userFinanceService;
            _budgetCategoryRepository = budgetCategoryRepository;
            _userSessionService = userSessionService;
            _householdFinanceService = householdFinanceService;
        }

        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            return _userSessionService.CurrentUser!.Id;
        }
        private static IncomeTarget ParseIncomeTarget(string? value)
        {
            if (!Enum.TryParse<IncomeTarget>(value, out var target))
                throw new InvalidOperationException("Income target must be provided for income records.");

            return target;
        }

        /// <summary>
        /// Retrieves all records for the specified household that were created by the currently authenticated user (child)
        /// and enriches them with corresponding category names.
        /// </summary>
        /// <param name="householdId">The household identifier.</param>
        /// <returns>
        /// A list of <see cref="Record"/> objects created by the current user within the specified household.
        /// Each record includes its associated category name when available.
        /// </returns>
        public async Task<List<Record>> GetAllByHouseholdAndCreatedByAsync(int householdId)
        {
            var userId = GetAuthenticatedUserId();

            var records = await _recordRepository.GetAllByHouseholdAndCreatedByAsync(householdId, userId);
            var categories = await _budgetCategoryRepository.GetAllByHouseholdAndCreatedByAsync(householdId, userId);

            var categoryDict = categories.ToDictionary(c => c.Id);

            foreach (var record in records)
            {
                if (record.CategoryId is int categoryId && categoryDict.TryGetValue(categoryId, out var category))
                {
                    record.CategoryName = category.Name;
                }
            }

            return records;
        }

        /// <summary>
        /// Retrieves all records for the specified household (shared context)
        /// and enriches them with corresponding category names.
        /// </summary>
        /// <param name="householdId">The household identifier.</param>
        /// <returns>
        /// A list of <see cref="Record"/> objects associated with the specified household.
        /// Each record includes its associated category name when available.
        /// </returns>
        public async Task<List<Record>> GetAllByHouseholdIdAsync(int householdId)
        {
            var records = await _recordRepository.GetAllByHouseholdIdAsync(householdId);
            var categories = await _budgetCategoryRepository.GetAllByHouseholdIdAsync(householdId);

            var categoryDict = categories.ToDictionary(c => c.Id);

            foreach (var record in records)
            {
                if (record.CategoryId is int categoryId && categoryDict.TryGetValue(categoryId, out var category))
                {
                    record.CategoryName = category.Name;
                }
            }

            return records;
        }

        /// <summary>
        /// Retrieves all records owned by the currently authenticated user (personal context)
        /// and enriches them with corresponding category names.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Record"/> objects owned by the current user.
        /// Each record includes its associated category name when available.
        /// </returns>
        public async Task<List<Record>> GetAllByOwnerAsync()
        {
            var userId = GetAuthenticatedUserId();

            var records = await _recordRepository.GetAllByOwnerAsync(userId);
            var categories = await _budgetCategoryRepository.GetAllByOwnerAsync(userId);

            var categoryDict = categories.ToDictionary(c => c.Id);

            foreach (var record in records)
            {
                if (record.CategoryId is int categoryId && categoryDict.TryGetValue(categoryId, out var category))
                {
                    record.CategoryName = category.Name;
                }
            }

            return records;
        }

        /// <summary>
        /// Adds a new financial record and applies its effect to the corresponding finance context
        /// (user or household) based on its scope and type.
        /// </summary>
        /// <param name="record">The record to be added.</param>
        /// <param name="category">The category associated with the record.</param>
        /// <returns>The identifier of the newly created record.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the record amount is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the record scope does not match the category scope,
        /// or when the record type is invalid,
        /// or when an income record has an invalid or missing income target.
        /// </exception>
        public async Task<int> AddRecordAsync(Record record, BudgetCategory category)
        {
            if (record.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(record.Amount), "Amount must be greater than zero.");

            if (record.Scope != category.Scope)
                throw new InvalidOperationException("Record scope does not match category scope.");

            switch (record.Scope)
            {
                case RecordConstants.Scopes.Shared:
                    switch (record.Type)
                    {
                        case RecordConstants.Types.Expense:
                            await _householdFinanceService.ApplyExpenseAsync(record.Amount, IncomeTarget.Balance);
                            break;
                        case RecordConstants.Types.Income:
                            await _householdFinanceService.ApplyIncomeAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget));
                            break;
                        default:
                            throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                    }
                    return await _recordRepository.AddAsync(record);
                case RecordConstants.Scopes.Personal:
                    switch (record.Type)
                    {
                        case RecordConstants.Types.Expense:
                            await _userFinanceService.ApplyExpenseAsync(record.Amount, IncomeTarget.Balance);
                            break;
                        case RecordConstants.Types.Income:
                            await _userFinanceService.ApplyIncomeAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget));
                            break;
                        default:
                            throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                    }
                    return await _recordRepository.AddAsync(record);

                default:
                    throw new InvalidOperationException("Invalid record scope. Scope must be either 'Shared' or 'Personal'.");
            }
        }

        /// <summary>
        /// Deletes an existing financial record and reverses its effect on the corresponding finance context
        /// (user or household) based on its scope and type.
        /// </summary>
        /// <param name="record">The record to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the record scope or type is invalid,
        /// or when an income record has an invalid or missing income target.
        /// </exception>
        public async Task DeleteRecordAsync(Record record)
        {
            switch (record.Scope)
            {
                case RecordConstants.Scopes.Shared:
                    switch (record.Type)
                    {
                        case RecordConstants.Types.Expense:
                            await _householdFinanceService.ApplyIncomeAsync(record.Amount, IncomeTarget.Balance);
                            break;
                        case RecordConstants.Types.Income:
                            await _householdFinanceService.ApplyExpenseAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget));
                            break;
                        default:
                            throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                    }
                    break;
                case RecordConstants.Scopes.Personal:
                    switch (record.Type)
                    {
                        case RecordConstants.Types.Expense:
                            await _userFinanceService.ApplyIncomeAsync(record.Amount, IncomeTarget.Balance);
                            break;
                        case RecordConstants.Types.Income:
                            await _userFinanceService.ApplyExpenseAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget));
                            break;
                        default:
                            throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                    }
                    break;

                default:
                    throw new InvalidOperationException("Invalid record scope. Scope must be either 'Shared' or 'Personal'.");
            }
            await _recordRepository.DeleteAsync(record.Id);
        }

        public async Task<List<Record>> GetRecordsByCategoryIdAsync(int categoryId)
        {
            return await _recordRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<List<Record>> GetRecordsByPeriodAsync(DateTime from, DateTime to, int? householdId)
        {
            return await _recordRepository.GetByPeriodAsync(from, to, householdId, GetAuthenticatedUserId());
        }

        public async Task UpdateRecordAsync(Record record)
        {
            await _recordRepository.UpdateAsync(record);
        }
    }
}
