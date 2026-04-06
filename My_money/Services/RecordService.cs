using My_money.Constants;
using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

        public RecordService(
            IRecordRepository recordRepository,
            IUserFinanceService userFinanceService,
            IBudgetCategoryRepository budgetCategoryRepository,
            IUserSessionService userSessionService,
            IHouseholdFinanceService householdFinanceService)
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

        public async Task<int> AddRecordAsync(Record record, BudgetCategory category)
        {
            if (record.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(record.Amount), "Amount must be greater than zero.");

            if (record.Scope != category.Scope)
                throw new InvalidOperationException("Record scope does not match category scope.");

            var userId = GetAuthenticatedUserId();
            record.CreatedByUserId = userId;

            using var connection = new SQLiteConnection(_recordRepository.ConnectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (record.Scope == RecordConstants.Scopes.Shared)
                {
                    record.OwnerUserId = null;

                    if (record.Type == RecordConstants.Types.Expense)
                        await _householdFinanceService.ApplyExpenseAsync(record.Amount, IncomeTarget.Balance, record.HouseholdId, connection, transaction);
                    else if (record.Type == RecordConstants.Types.Income)
                        await _householdFinanceService.ApplyIncomeAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget), record.HouseholdId, connection, transaction);
                    else
                        throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                }
                else if (record.Scope == RecordConstants.Scopes.Personal)
                {
                    record.OwnerUserId = userId;

                    if (record.Type == RecordConstants.Types.Expense)
                        await _userFinanceService.ApplyExpenseAsync(record.Amount, IncomeTarget.Balance, connection, transaction);
                    else if (record.Type == RecordConstants.Types.Income)
                        await _userFinanceService.ApplyIncomeAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget), connection, transaction);
                    else
                        throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                }
                else
                {
                    throw new InvalidOperationException("Invalid record scope. Scope must be either 'Shared' or 'Personal'.");
                }

                var recordId = await _recordRepository.AddAsync(record, connection, transaction);
                transaction.Commit();
                return recordId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeleteRecordAsync(Record record)
        {
            using var connection = new SQLiteConnection(_recordRepository.ConnectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (record.Scope == RecordConstants.Scopes.Shared)
                {
                    if (record.Type == RecordConstants.Types.Expense)
                        await _householdFinanceService.ApplyIncomeAsync(record.Amount, IncomeTarget.Balance, record.HouseholdId, connection, transaction);
                    else if (record.Type == RecordConstants.Types.Income)
                        await _householdFinanceService.ApplyExpenseAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget), record.HouseholdId, connection, transaction);
                    else
                        throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                }
                else if (record.Scope == RecordConstants.Scopes.Personal)
                {
                    if (record.Type == RecordConstants.Types.Expense)
                        await _userFinanceService.ApplyIncomeAsync(record.Amount, IncomeTarget.Balance, connection, transaction);
                    else if (record.Type == RecordConstants.Types.Income)
                        await _userFinanceService.ApplyExpenseAsync(record.Amount, ParseIncomeTarget(record.IncomeTarget), connection, transaction);
                    else
                        throw new InvalidOperationException("Invalid record type. Type must be either 'Expense' or 'Income'.");
                }
                else
                {
                    throw new InvalidOperationException("Invalid record scope. Scope must be either 'Shared' or 'Personal'.");
                }

                await _recordRepository.DeleteAsync(record.Id, connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task UpdateRecordAsync(Record record)
        {
            await _recordRepository.UpdateAsync(record);
        }

        public async Task<List<Record>> GetRecordsByCategoryIdAsync(int categoryId)
        {
            return await _recordRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<List<Record>> GetRecordsByPeriodAsync(DateTime from, DateTime to, CategoryFilterType categoryFilterType, int? householdId)
        {
            var userId = GetAuthenticatedUserId();

            return categoryFilterType switch
            {
                CategoryFilterType.Household => householdId.HasValue
                    ? await _recordRepository.GetHouseholdByPeriodAsync(from, to, householdId.Value)
                    : throw new ArgumentNullException(nameof(householdId)),
                CategoryFilterType.Personal => await _recordRepository.GetPersonalByPeriodAsync(from, to, userId),
                CategoryFilterType.Child => householdId.HasValue
                    ? await _recordRepository.GetChildByPeriodAsync(from, to, householdId.Value, userId)
                    : throw new ArgumentNullException(nameof(householdId)),
                _ => throw new ArgumentOutOfRangeException(nameof(categoryFilterType))
            };
        }

        private static IncomeTarget ParseIncomeTarget(string? value)
        {
            if (!Enum.TryParse<IncomeTarget>(value, out var target))
                throw new InvalidOperationException("Income target must be provided for income records.");

            return target;
        }
    }
}
