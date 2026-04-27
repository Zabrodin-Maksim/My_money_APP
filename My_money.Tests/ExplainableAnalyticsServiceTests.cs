using My_money.Constants;
using My_money.Enums;
using My_money.Model;
using My_money.Services;
using My_money.Services.IServices;
using System.Data.SQLite;
using ModelRecord = My_money.Model.Record;

namespace My_money.Tests;

public class ExplainableAnalyticsServiceTests
{
    #region Test Cases
    [Fact]
    public async Task BuildDashboardAnalyticsAsync_WithOneWarningAndOneAdvice_ReturnsGuardedRisk()
    {
        DateTime anchorDate = new(2026, 4, 20);

        var service = CreateService(
            categories:
            [
                CreateCategory(1, "Mortgage", 30000m, 10000m),
                CreateCategory(2, "Food", 13500m, 7220m)
            ],
            records:
            [
                CreateExpense(1, 10000m, new DateTime(2026, 4, 5)),
                CreateExpense(2, 7220m, new DateTime(2026, 4, 11))
            ],
            goals:
            [
                CreateGoal("Toy", 40m, 1000m),
                CreateGoal("Trip", 20m, 1000m),
                CreateGoal("Bike", 60m, 1500m),
                CreateGoal("Laptop", 40m, 1500m)
            ],
            householdFinance: new HouseholdFinance
            {
                Id = 1,
                HouseholdId = 1,
                Savings = 5000m,
                Balance = 12000m
            });

        DashboardAnalyticsSnapshot snapshot = await service.BuildDashboardAnalyticsAsync(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 30),
            anchorDate,
            CategoryFilterType.Household,
            usesHouseholdFinance: true,
            householdId: 1);

        AnalyticsMetric riskMetric = Assert.Single(snapshot.Metrics, metric => metric.Title == "Risk level");
        Assert.Equal("Guarded", riskMetric.Value);

        Assert.Contains(snapshot.Insights, insight => insight.Title == "Financial cushion is thin");
        Assert.Contains(snapshot.Insights, insight => insight.Title == "Savings goals need stronger momentum");
        Assert.Contains(snapshot.Insights, insight => insight.Title == "Budget pace is under control");
    }

    [Fact]
    public async Task BuildDashboardAnalyticsAsync_WithTwoWarnings_ReturnsHighRisk()
    {
        DateTime anchorDate = new(2026, 4, 20);

        var service = CreateService(
            categories:
            [
                CreateCategory(1, "Mortgage", 8000m, 12000m),
                CreateCategory(2, "Food", 3000m, 3500m),
                CreateCategory(3, "Entertainment", 1500m, 2200m)
            ],
            records:
            [
                CreateExpense(1, 12000m, new DateTime(2026, 4, 5)),
                CreateExpense(2, 3500m, new DateTime(2026, 4, 11)),
                CreateExpense(3, 2200m, new DateTime(2026, 4, 19))
            ],
            goals:
            [
                CreateGoal("Emergency", 100m, 5000m)
            ],
            householdFinance: new HouseholdFinance
            {
                Id = 1,
                HouseholdId = 1,
                Savings = 2000m,
                Balance = -500m
            });

        DashboardAnalyticsSnapshot snapshot = await service.BuildDashboardAnalyticsAsync(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 30),
            anchorDate,
            CategoryFilterType.Household,
            usesHouseholdFinance: true,
            householdId: 1);

        AnalyticsMetric riskMetric = Assert.Single(snapshot.Metrics, metric => metric.Title == "Risk level");
        Assert.Equal("High", riskMetric.Value);

        Assert.Contains(snapshot.Insights, insight => insight.Title == "Financial cushion is thin");
        Assert.Contains(snapshot.Insights, insight => insight.Title == "Balance is below zero");
    }

    [Fact]
    public async Task BuildDashboardAnalyticsAsync_WithOnlyCurrentMonthHistory_ReturnsStartingPointTrend()
    {
        DateTime anchorDate = new(2026, 4, 20);

        var service = CreateService(
            categories:
            [
                CreateCategory(1, "Food", 5000m, 3000m)
            ],
            records:
            [
                CreateExpense(1, 3000m, new DateTime(2026, 4, 10))
            ],
            goals: [],
            householdFinance: new HouseholdFinance
            {
                Id = 1,
                HouseholdId = 1,
                Savings = 1500m,
                Balance = 7000m
            });

        DashboardAnalyticsSnapshot snapshot = await service.BuildDashboardAnalyticsAsync(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 30),
            anchorDate,
            CategoryFilterType.Household,
            usesHouseholdFinance: true,
            householdId: 1);

        AnalyticsMetric trendMetric = Assert.Single(snapshot.Metrics, metric => metric.Title == "Monthly trend");
        Assert.Equal("Starting point", trendMetric.Value);
    }

    [Fact]
    public async Task BuildDashboardAnalyticsAsync_WithTrackedSpendAndNoPlan_ReturnsPlanningAdvice()
    {
        DateTime anchorDate = new(2026, 4, 20);

        var service = CreateService(
            categories:
            [
                CreateCategory(1, "Other", 0m, 2200m)
            ],
            records:
            [
                CreateExpense(1, 2200m, new DateTime(2026, 4, 9))
            ],
            goals: [],
            householdFinance: new HouseholdFinance
            {
                Id = 1,
                HouseholdId = 1,
                Savings = 10000m,
                Balance = 4000m
            });

        DashboardAnalyticsSnapshot snapshot = await service.BuildDashboardAnalyticsAsync(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 30),
            anchorDate,
            CategoryFilterType.Household,
            usesHouseholdFinance: true,
            householdId: 1);

        Assert.Contains(snapshot.Insights, insight => insight.Title == "Spending is tracked but not planned");
    }
    #endregion

    #region Factory Helpers
    private static ExplainableAnalyticsService CreateService(
        List<BudgetCategory> categories,
        List<ModelRecord> records,
        List<SavingsGoal> goals,
        HouseholdFinance householdFinance)
    {
        var session = new FakeUserSessionService();
        session.StartSession(
            new User
            {
                Id = 7,
                Email = "test@example.com",
                PasswordHash = "hash",
                DisplayName = "Tester",
                IsActive = 1
            },
            new HouseholdMember
            {
                Id = 1,
                HouseholdId = 1,
                UserId = 7,
                Role = "Admin",
                CanManageBudget = 1,
                CanManageMembers = 1
            });

        return new ExplainableAnalyticsService(
            new FakeBudgetCategoryService(categories),
            new FakeRecordService(records),
            new FakeSavingsGoalService(goals),
            new FakeUserFinanceService(),
            new FakeHouseholdFinanceService(householdFinance),
            session);
    }

    private static BudgetCategory CreateCategory(int id, string name, decimal planned, decimal spent)
    {
        return new BudgetCategory
        {
            Id = id,
            Name = name,
            Plan = planned,
            HouseholdId = 1,
            OwnerUserId = null,
            CreatedByUserId = 7,
            Scope = RecordConstants.Scopes.Shared,
            PlanByPeriod = planned,
            SpendByPeriod = spent,
            RemainingByPeriod = planned - spent
        };
    }

    private static ModelRecord CreateExpense(int categoryId, decimal amount, DateTime occurredAt)
    {
        return new ModelRecord
        {
            Id = Guid.NewGuid().GetHashCode(),
            Amount = amount,
            CategoryId = categoryId,
            CategoryName = string.Empty,
            DateTimeOccurred = occurredAt,
            Description = null,
            HouseholdId = 1,
            OwnerUserId = null,
            CreatedByUserId = 7,
            Scope = RecordConstants.Scopes.Shared,
            Type = RecordConstants.Types.Expense,
            IncomeTarget = null
        };
    }

    private static SavingsGoal CreateGoal(string name, decimal have, decimal need)
    {
        return new SavingsGoal
        {
            Id = Guid.NewGuid().GetHashCode(),
            GoalName = name,
            Have = have,
            Need = need,
            HouseholdId = 1,
            OwnerUserId = null,
            CreatedByUserId = 7,
            Scope = RecordConstants.Scopes.Shared
        };
    }
    #endregion

    #region Fakes
    private sealed class FakeBudgetCategoryService(List<BudgetCategory> categories) : IBudgetCategoryService
    {
        public Task<List<BudgetCategory>> GetAllByHouseholdAndCreatedByAsync(int householdId) => Task.FromResult(categories);
        public Task<List<BudgetCategory>> GetAllByHouseholdIdAsync(int householdId) => Task.FromResult(categories);
        public Task<List<BudgetCategory>> GetAllByOwnerAsync() => Task.FromResult(categories);
        public Task<List<BudgetCategory>> GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to, CategoryFilterType categoryFilterType, int? householdId) => Task.FromResult(categories);
        public Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int id) => Task.FromResult(categories.FirstOrDefault(x => x.Id == id));
        public Task<int> AddBudgetCategoryAsync(BudgetCategory category) => throw new NotSupportedException();
        public Task UpdateBudgetCategoryAsync(BudgetCategory category) => throw new NotSupportedException();
        public Task DeleteBudgetCategoryAsync(BudgetCategory category) => throw new NotSupportedException();
        public Task<BudgetCategory?> GetBudgetCategoryByNameAsync(string name, int householdId, int? ownerUserId, string scope) => throw new NotSupportedException();
    }

    private sealed class FakeRecordService(List<ModelRecord> records) : IRecordService
    {
        public Task<List<ModelRecord>> GetAllByHouseholdAndCreatedByAsync(int householdId) => Task.FromResult(records);
        public Task<List<ModelRecord>> GetAllByHouseholdIdAsync(int householdId) => Task.FromResult(records);
        public Task<List<ModelRecord>> GetAllByOwnerAsync() => Task.FromResult(records);
        public Task<int> AddRecordAsync(ModelRecord record, BudgetCategory category) => throw new NotSupportedException();
        public Task DeleteRecordAsync(ModelRecord record) => throw new NotSupportedException();
        public Task UpdateRecordAsync(ModelRecord record) => throw new NotSupportedException();
        public Task<List<ModelRecord>> GetRecordsByCategoryIdAsync(int categoryId) => Task.FromResult(records.Where(record => record.CategoryId == categoryId).ToList());
        public Task<List<ModelRecord>> GetRecordsByPeriodAsync(DateTime from, DateTime to, CategoryFilterType categoryFilterType, int? householdId)
        {
            return Task.FromResult(records
                .Where(record => record.DateTimeOccurred.HasValue
                    && record.DateTimeOccurred.Value >= from
                    && record.DateTimeOccurred.Value <= to)
                .ToList());
        }
    }

    private sealed class FakeSavingsGoalService(List<SavingsGoal> goals) : ISavingsGoalService
    {
        public Task<List<SavingsGoal>> GetAllByHouseholdAndCreatedByAsync(int householdId) => Task.FromResult(goals);
        public Task<List<SavingsGoal>> GetAllByHouseholdIdAsync(int householdId) => Task.FromResult(goals);
        public Task<List<SavingsGoal>> GetAllByOwnerAsync() => Task.FromResult(goals);
        public Task<SavingsGoal?> GetSavingsGoal(int id) => Task.FromResult(goals.FirstOrDefault(goal => goal.Id == id));
        public Task<int> AddSavingsGoal(SavingsGoal goal) => throw new NotSupportedException();
        public Task UpdateSavingsGoal(SavingsGoal goal) => throw new NotSupportedException();
        public Task DeleteSavingsGoal(int id) => throw new NotSupportedException();
    }

    private sealed class FakeUserFinanceService : IUserFinanceService
    {
        public Task<UserFinance?> GetByUserIdAsync(int userId) => Task.FromResult<UserFinance?>(new UserFinance { Id = 1, UserId = userId, Savings = 0m, Balance = 0m });
        public Task<int> AddUserFinanceAsync(UserFinance userFinance) => throw new NotSupportedException();
        public Task ApplyExpenseAsync(decimal amount, IncomeTarget target) => throw new NotSupportedException();
        public Task ApplyExpenseAsync(decimal amount, IncomeTarget target, SQLiteConnection connection, SQLiteTransaction transaction) => throw new NotSupportedException();
        public Task ApplyIncomeAsync(decimal amount, IncomeTarget target) => throw new NotSupportedException();
        public Task ApplyIncomeAsync(decimal amount, IncomeTarget target, SQLiteConnection connection, SQLiteTransaction transaction) => throw new NotSupportedException();
        public Task UpdateUserFinanceAsync(decimal? savings, decimal? balance) => throw new NotSupportedException();
        public Task DeleteUserFinance(int id) => throw new NotSupportedException();
    }

    private sealed class FakeHouseholdFinanceService(HouseholdFinance finance) : IHouseholdFinanceService
    {
        public Task<HouseholdFinance?> GetHouseholdFinanceByIdAsync(int id) => Task.FromResult<HouseholdFinance?>(finance);
        public Task<HouseholdFinance?> GetHouseholdFinanceByHouseholdIdAsync(int householdId) => Task.FromResult<HouseholdFinance?>(finance);
        public Task<int> AddHouseholdFinanceAsync(HouseholdFinance finance) => throw new NotSupportedException();
        public Task UpdateHouseholdFinanceAsync(decimal? savings, decimal? balance) => throw new NotSupportedException();
        public Task ApplyExpenseAsync(decimal amount, IncomeTarget target) => throw new NotSupportedException();
        public Task ApplyExpenseAsync(decimal amount, IncomeTarget target, int householdId, SQLiteConnection connection, SQLiteTransaction transaction) => throw new NotSupportedException();
        public Task ApplyIncomeAsync(decimal amount, IncomeTarget target) => throw new NotSupportedException();
        public Task ApplyIncomeAsync(decimal amount, IncomeTarget target, int householdId, SQLiteConnection connection, SQLiteTransaction transaction) => throw new NotSupportedException();
        public Task DeleteHouseholdFinanceAsync(int id) => throw new NotSupportedException();
    }

    private sealed class FakeUserSessionService : IUserSessionService
    {
        public User? CurrentUser { get; private set; }
        public HouseholdMember? CurrentHouseholdMember { get; private set; }
        public bool IsAuthenticated => CurrentUser is not null && CurrentHouseholdMember is not null;

        public void StartSession(User user, HouseholdMember householdMember)
        {
            CurrentUser = user;
            CurrentHouseholdMember = householdMember;
        }

        public void EndSession()
        {
            CurrentUser = null;
            CurrentHouseholdMember = null;
        }
    }
    #endregion
}
