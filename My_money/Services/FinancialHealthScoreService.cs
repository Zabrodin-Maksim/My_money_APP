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
    public class FinancialHealthScoreService : IFinancialHealthScoreService
    {
        private const int NeutralComponentScore = 60;

        #region Dependency Injection Repositories
        private readonly IRecordRepository _recordRepository;
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly ISavingsGoalRepository _savingsGoalRepository;
        private readonly IUserFinanceRepository _userFinanceRepository;
        #endregion

        public FinancialHealthScoreService(
            IRecordRepository recordRepository,
            IBudgetCategoryRepository budgetCategoryRepository,
            ISavingsGoalRepository savingsGoalRepository,
            IUserFinanceRepository userFinanceRepository)
        {
            #region Dependency Injection
            _recordRepository = recordRepository;
            _budgetCategoryRepository = budgetCategoryRepository;
            _savingsGoalRepository = savingsGoalRepository;
            _userFinanceRepository = userFinanceRepository;
            #endregion
        }

        #region Public Methods
        public async Task<int> GetFinancialHealthScoreAsync(HouseholdMember member)
        {
            #region Load Member Data
            bool isChild = member.Role == nameof(HouseholdMemberRole.Child);
            Task<List<Record>> recordsTask = isChild
                ? _recordRepository.GetAllByHouseholdAndCreatedByAsync(member.HouseholdId, member.UserId)
                : _recordRepository.GetAllByOwnerAsync(member.UserId);

            Task<List<BudgetCategory>> categoriesTask = isChild
                ? _budgetCategoryRepository.GetAllByHouseholdAndCreatedByAsync(member.HouseholdId, member.UserId)
                : _budgetCategoryRepository.GetAllByOwnerAsync(member.UserId);

            Task<List<SavingsGoal>> goalsTask = isChild
                ? _savingsGoalRepository.GetAllByHouseholdAndCreatedByAsync(member.HouseholdId, member.UserId)
                : _savingsGoalRepository.GetAllByOwnerAsync(member.UserId);

            Task<UserFinance?> userFinanceTask = isChild
                ? Task.FromResult<UserFinance?>(null)
                : _userFinanceRepository.GetByUserIdAsync(member.UserId);

            await Task.WhenAll(recordsTask, categoriesTask, goalsTask, userFinanceTask);

            var allRecords = recordsTask.Result;
            var allCategories = categoriesTask.Result;
            var allGoals = goalsTask.Result;
            #endregion

            #region Component Scores
            int budgetScore = CalculateBudgetScore(allCategories, allRecords);
            int savingsScore = CalculateSavingsScore(allRecords, allGoals, userFinanceTask.Result, isChild);
            int stabilityScore = CalculateExpenseStabilityScore(allRecords);
            int goalsScore = CalculateGoalsScore(allGoals);
            #endregion

            #region Weighted Total
            decimal weightedScore =
                budgetScore * 0.35m +
                savingsScore * 0.25m +
                stabilityScore * 0.20m +
                goalsScore * 0.20m;

            return ClampScore((int)Math.Round(weightedScore, MidpointRounding.AwayFromZero));
            #endregion
        }
        #endregion

        #region Score Components
        private static int CalculateBudgetScore(IReadOnlyCollection<BudgetCategory> categories, IReadOnlyCollection<Record> records)
        {
            decimal totalPlanned = categories.Sum(category => category.Plan);

            if (totalPlanned <= 0m)
            {
                return NeutralComponentScore;
            }

            DateTime today = DateTime.Today;
            DateTime monthStart = new(today.Year, today.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            var currentMonthExpenses = records
                .Where(record => record.Type == RecordConstants.Types.Expense
                    && record.CategoryId.HasValue
                    && record.DateTimeOccurred.HasValue
                    && record.DateTimeOccurred.Value >= monthStart
                    && record.DateTimeOccurred.Value <= monthEnd);

            var spendByCategory = currentMonthExpenses
                .GroupBy(record => record.CategoryId!.Value)
                .ToDictionary(group => group.Key, group => group.Sum(record => record.Amount));

            decimal spentAgainstPlan = categories.Sum(category =>
                spendByCategory.TryGetValue(category.Id, out decimal spend) ? spend : 0m);

            decimal usageRatio = spentAgainstPlan / totalPlanned;

            if (usageRatio <= 1m)
            {
                return 100;
            }

            decimal overrunRatio = usageRatio - 1m;
            return ClampScore(100 - (int)Math.Round(overrunRatio * 160m, MidpointRounding.AwayFromZero));
        }

        private static int CalculateSavingsScore(
            IReadOnlyCollection<Record> records,
            IReadOnlyCollection<SavingsGoal> goals,
            UserFinance? userFinance,
            bool isChild)
        {
            decimal averageMonthlyExpense = GetTrailingMonthlyExpenseTotals(records, 3).Average();

            if (!isChild && userFinance is not null)
            {
                if (averageMonthlyExpense <= 0m)
                {
                    return userFinance.Savings > 0m ? 90 : NeutralComponentScore;
                }

                decimal reserveMonths = userFinance.Savings / averageMonthlyExpense;
                int reserveScore = ClampScore((int)Math.Round((reserveMonths / 3m) * 100m, MidpointRounding.AwayFromZero));

                if (userFinance.Balance < 0m)
                {
                    reserveScore = ClampScore(reserveScore - 10);
                }

                return reserveScore;
            }

            DateTime recentThreshold = DateTime.Today.AddDays(-90);
            var recentIncomeRecords = records
                .Where(record => record.Type == RecordConstants.Types.Income
                    && record.DateTimeOccurred.HasValue
                    && record.DateTimeOccurred.Value >= recentThreshold)
                .ToList();

            if (recentIncomeRecords.Count > 0)
            {
                decimal totalIncome = recentIncomeRecords.Sum(record => record.Amount);
                decimal savingsIncome = recentIncomeRecords
                    .Where(record => string.Equals(record.IncomeTarget, nameof(IncomeTarget.Savings), StringComparison.Ordinal))
                    .Sum(record => record.Amount);

                if (totalIncome > 0m)
                {
                    decimal savingsShare = savingsIncome / totalIncome;
                    return ClampScore((int)Math.Round((savingsShare / 0.25m) * 100m, MidpointRounding.AwayFromZero));
                }
            }

            if (goals.Count > 0)
            {
                return Math.Max(NeutralComponentScore, CalculateGoalsScore(goals));
            }

            return NeutralComponentScore;
        }

        private static int CalculateExpenseStabilityScore(IReadOnlyCollection<Record> records)
        {
            var monthlyExpenses = GetTrailingMonthlyExpenseTotals(records, 3);

            if (monthlyExpenses.All(total => total <= 0m))
            {
                return NeutralComponentScore;
            }

            decimal average = monthlyExpenses.Average();

            if (average <= 0m)
            {
                return NeutralComponentScore;
            }

            double variance = monthlyExpenses
                .Select(total => Math.Pow((double)(total - average), 2))
                .Average();

            double standardDeviation = Math.Sqrt(variance);
            double coefficientOfVariation = standardDeviation / (double)average;

            return ClampScore(100 - (int)Math.Round(coefficientOfVariation * 120d, MidpointRounding.AwayFromZero));
        }

        private static int CalculateGoalsScore(IReadOnlyCollection<SavingsGoal> goals)
        {
            var activeGoals = goals
                .Where(goal => goal.Need > 0m)
                .ToList();

            if (activeGoals.Count == 0)
            {
                return NeutralComponentScore;
            }

            decimal averageProgress = activeGoals.Average(goal => Math.Min(goal.Percent, 100m));
            return ClampScore((int)Math.Round(averageProgress, MidpointRounding.AwayFromZero));
        }
        #endregion

        #region Helper Methods
        private static List<decimal> GetTrailingMonthlyExpenseTotals(IReadOnlyCollection<Record> records, int monthCount)
        {
            var monthlyExpenses = new List<decimal>(monthCount);
            DateTime monthCursor = new(DateTime.Today.Year, DateTime.Today.Month, 1);

            for (int i = 0; i < monthCount; i++)
            {
                DateTime monthStart = monthCursor.AddMonths(-i);
                DateTime monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                decimal monthlyExpense = records
                    .Where(record => record.Type == RecordConstants.Types.Expense
                        && record.DateTimeOccurred.HasValue
                        && record.DateTimeOccurred.Value >= monthStart
                        && record.DateTimeOccurred.Value <= monthEnd)
                    .Sum(record => record.Amount);

                monthlyExpenses.Add(monthlyExpense);
            }

            return monthlyExpenses;
        }

        private static int ClampScore(int score)
        {
            return Math.Clamp(score, 0, 100);
        }
        #endregion
    }
}
