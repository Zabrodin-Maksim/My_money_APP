using My_money.Constants;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class ExplainableAnalyticsService : IExplainableAnalyticsService
    {
        private const string InsightWarning = "Warning";
        private const string InsightObservation = "Observation";
        private const string InsightAdvice = "Advice";
        private const string InsightPraise = "Praise";

        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IRecordService _recordService;
        private readonly ISavingsGoalService _savingsGoalService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IHouseholdFinanceService _householdFinanceService;
        private readonly IUserSessionService _userSessionService;
        #endregion

        public ExplainableAnalyticsService(
            IBudgetCategoryService budgetCategoryService,
            IRecordService recordService,
            ISavingsGoalService savingsGoalService,
            IUserFinanceService userFinanceService,
            IHouseholdFinanceService householdFinanceService,
            IUserSessionService userSessionService)
        {
            #region Dependency Injection
            _budgetCategoryService = budgetCategoryService;
            _recordService = recordService;
            _savingsGoalService = savingsGoalService;
            _userFinanceService = userFinanceService;
            _householdFinanceService = householdFinanceService;
            _userSessionService = userSessionService;
            #endregion
        }

        #region Public Methods
        public async Task<DashboardAnalyticsSnapshot> BuildDashboardAnalyticsAsync(
            DateTime periodFrom,
            DateTime periodTo,
            DateTime trendAnchorDate,
            CategoryFilterType categoryFilterType,
            bool usesHouseholdFinance,
            int? householdId)
        {
            #region Data Loading
            int userId = GetAuthenticatedUserId();
            CategoryFilterType financeAlignedFilter = usesHouseholdFinance ? CategoryFilterType.Household : categoryFilterType;

            var currentCategoriesTask = _budgetCategoryService.GetAllBudgetCategoriesByPeriodAsync(periodFrom, periodTo, categoryFilterType, householdId);
            var currentRecordsTask = _recordService.GetRecordsByPeriodAsync(periodFrom, periodTo, categoryFilterType, householdId);
            var monthlyTrendTask = GetMonthlyTrendAsync(trendAnchorDate, categoryFilterType, householdId, 6);
            var cushionTrendTask = GetMonthlyTrendAsync(trendAnchorDate, financeAlignedFilter, householdId, 3);
            var goalsTask = LoadGoalsAsync(usesHouseholdFinance, householdId);
            var financeTask = LoadFinanceAsync(usesHouseholdFinance, userId, householdId);

            await Task.WhenAll(currentCategoriesTask, currentRecordsTask, monthlyTrendTask, cushionTrendTask, goalsTask, financeTask);

            var categories = currentCategoriesTask.Result;
            var currentRecords = currentRecordsTask.Result;
            var monthlyTrend = monthlyTrendTask.Result;
            var cushionTrend = cushionTrendTask.Result;
            var goals = goalsTask.Result;
            var financeSnapshot = financeTask.Result;
            #endregion

            #region Calculated Metrics
            decimal totalSpend = categories.Sum(category => category.SpendByPeriod ?? 0m);
            decimal totalPlanned = categories.Sum(category => category.PlanByPeriod ?? category.Plan);
            decimal averageGoalProgress = GetAverageGoalProgress(goals);
            decimal averageMonthlyExpense = cushionTrend.Count == 0 ? 0m : cushionTrend.Average(point => point.Amount);
            decimal cushionMonths = averageMonthlyExpense <= 0m ? 0m : financeSnapshot.Savings / averageMonthlyExpense;

            var insights = BuildInsights(
                categories,
                currentRecords,
                monthlyTrend,
                goals,
                financeSnapshot.Balance,
                financeSnapshot.Savings,
                totalSpend,
                totalPlanned,
                averageMonthlyExpense,
                cushionMonths);

            var metrics = BuildMetrics(
                monthlyTrend,
                goals,
                financeSnapshot.Savings,
                averageMonthlyExpense,
                averageGoalProgress,
                insights,
                cushionMonths);
            #endregion

            return new DashboardAnalyticsSnapshot
            {
                OverviewText = BuildOverviewText(insights, usesHouseholdFinance),
                Metrics = metrics,
                Insights = insights,
                MonthlyTrend = monthlyTrend
            };
        }
        #endregion

        #region Insight Builders
        private static List<AnalyticsInsight> BuildInsights(
            IReadOnlyCollection<BudgetCategory> categories,
            IReadOnlyCollection<Record> currentRecords,
            IReadOnlyCollection<MonthlySpendTrendPoint> monthlyTrend,
            IReadOnlyCollection<SavingsGoal> goals,
            decimal balance,
            decimal savings,
            decimal totalSpend,
            decimal totalPlanned,
            decimal averageMonthlyExpense,
            decimal cushionMonths)
        {
            var insights = new List<AnalyticsInsight>();

            var overruns = categories
                .Where(category => (category.PlanByPeriod ?? category.Plan) > 0m && (category.SpendByPeriod ?? 0m) > (category.PlanByPeriod ?? category.Plan))
                .OrderByDescending(category => (category.SpendByPeriod ?? 0m) - (category.PlanByPeriod ?? category.Plan))
                .ToList();

            if (overruns.Count > 0)
            {
                var category = overruns.First();
                decimal planned = category.PlanByPeriod ?? category.Plan;
                decimal spent = category.SpendByPeriod ?? 0m;
                decimal overrunPct = planned == 0m ? 0m : Math.Round(((spent - planned) / planned) * 100m, 0);

                insights.Add(new AnalyticsInsight
                {
                    Kind = InsightWarning,
                    Title = $"{category.Name} is over budget",
                    Detected = $"Spent {spent:C0} against a planned {planned:C0} in the selected period.",
                    WhyItMatters = $"{category.Name} is running {overrunPct}% above plan, which forces the rest of the budget to absorb the gap.",
                    SuggestedAction = $"Review the next purchases in {category.Name} first, or rebalance part of the plan from a category with spare room."
                });
            }
            else if (totalPlanned > 0m && totalSpend <= totalPlanned && totalSpend > 0m)
            {
                insights.Add(new AnalyticsInsight
                {
                    Kind = InsightPraise,
                    Title = "Budget pace is under control",
                    Detected = $"Tracked spending is {totalSpend:C0} against a planned {totalPlanned:C0}.",
                    WhyItMatters = "Staying inside the planned budget keeps later months from paying for current pressure.",
                    SuggestedAction = "Keep the same pacing and only rebalance categories if a new priority appears."
                });
            }
            else if (totalPlanned <= 0m && totalSpend > 0m)
            {
                insights.Add(new AnalyticsInsight
                {
                    Kind = InsightAdvice,
                    Title = "Spending is tracked but not planned",
                    Detected = $"There is {totalSpend:C0} of spend in the selected period, but no active budget target.",
                    WhyItMatters = "Without category limits, it becomes harder to tell whether the current pace is healthy or drifting.",
                    SuggestedAction = "Set a simple plan for the biggest categories first, even if the first numbers are only rough estimates."
                });
            }

            var topSpendCategory = categories
                .Where(category => (category.SpendByPeriod ?? 0m) > 0m)
                .OrderByDescending(category => category.SpendByPeriod ?? 0m)
                .FirstOrDefault();

            if (topSpendCategory is not null && totalSpend > 0m)
            {
                decimal share = Math.Round(((topSpendCategory.SpendByPeriod ?? 0m) / totalSpend) * 100m, 0);
                insights.Add(new AnalyticsInsight
                {
                    Kind = share >= 45m ? InsightObservation : InsightPraise,
                    Title = share >= 45m ? $"{topSpendCategory.Name} dominates the period" : $"{topSpendCategory.Name} is the main spend driver",
                    Detected = $"{topSpendCategory.Name} accounts for {share}% of tracked expenses in the current view.",
                    WhyItMatters = share >= 45m
                        ? "When one category dominates the month, small changes inside it have an outsized effect on total financial pressure."
                        : "Knowing the leading category helps focus attention where changes have the strongest impact.",
                    SuggestedAction = share >= 45m
                        ? $"Audit the recent purchases inside {topSpendCategory.Name} and decide which ones were essential versus optional."
                        : $"Use {topSpendCategory.Name} as the anchor category when you fine-tune next month's plan."
                });
            }

            if (monthlyTrend.Count >= 4)
            {
                decimal latest = monthlyTrend.Last().Amount;
                decimal trailingAverage = monthlyTrend.Take(monthlyTrend.Count - 1).TakeLast(3).Average(point => point.Amount);

                if (trailingAverage > 0m)
                {
                    decimal deltaPct = Math.Round(((latest - trailingAverage) / trailingAverage) * 100m, 0);

                    if (deltaPct >= 15m)
                    {
                        insights.Add(new AnalyticsInsight
                        {
                            Kind = InsightWarning,
                            Title = "Monthly spend is accelerating",
                            Detected = $"The latest month closed at {latest:C0}, which is {deltaPct}% above the trailing average.",
                            WhyItMatters = "When expenses climb for multiple months, overshoots become harder to reverse late in the cycle.",
                            SuggestedAction = "Compare the latest month with the previous three and freeze the categories that changed most until the pace normalises."
                        });
                    }
                    else if (deltaPct <= -10m)
                    {
                        insights.Add(new AnalyticsInsight
                        {
                            Kind = InsightPraise,
                            Title = "Monthly trend is cooling down",
                            Detected = $"The latest month closed at {latest:C0}, which is {Math.Abs(deltaPct):0}% below the trailing average.",
                            WhyItMatters = "A lower-than-usual month creates room for savings, debt reduction, or a stronger next-month buffer.",
                            SuggestedAction = "Keep the habits that helped this drop and redirect part of the freed-up cash to savings."
                        });
                    }
                    else
                    {
                        insights.Add(new AnalyticsInsight
                        {
                            Kind = InsightObservation,
                            Title = "Monthly trend is stable",
                            Detected = $"The latest month is tracking close to the recent average at {latest:C0}.",
                            WhyItMatters = "Stable months are easier to plan because fewer categories are changing at once.",
                            SuggestedAction = "No urgent correction is needed; just keep watching the categories that historically swing the most."
                        });
                    }
                }
            }

            if (averageMonthlyExpense > 0m)
            {
                if (cushionMonths < 1m)
                {
                    insights.Add(new AnalyticsInsight
                    {
                        Kind = InsightWarning,
                        Title = "Financial cushion is thin",
                        Detected = $"Current savings cover about {cushionMonths:0.0} months of average expenses.",
                        WhyItMatters = "A reserve below one month leaves very little room for unexpected bills or a temporary income drop.",
                        SuggestedAction = "Route part of the next income to savings first and aim for at least one full month of expense coverage."
                    });
                }
                else if (cushionMonths < 3m)
                {
                    insights.Add(new AnalyticsInsight
                    {
                        Kind = InsightAdvice,
                        Title = "Cushion is building, but not yet strong",
                        Detected = $"Current savings cover about {cushionMonths:0.0} months of average expenses.",
                        WhyItMatters = "One to three months of runway is a meaningful start, but still leaves pressure if a bigger shock lands.",
                        SuggestedAction = "Treat the buffer like a recurring goal until it reaches the three-month mark."
                    });
                }
                else
                {
                    insights.Add(new AnalyticsInsight
                    {
                        Kind = InsightPraise,
                        Title = "Emergency buffer is healthy",
                        Detected = $"Current savings cover about {cushionMonths:0.0} months of average expenses.",
                        WhyItMatters = "A multi-month reserve improves resilience and reduces the chance that a surprise expense turns into debt.",
                        SuggestedAction = "Keep protecting this reserve and avoid dipping into it for non-emergency spending."
                    });
                }
            }

            var activeGoals = goals
                .Where(goal => goal.Need > 0m)
                .OrderByDescending(goal => goal.Percent)
                .ToList();

            if (activeGoals.Count == 0)
            {
                insights.Add(new AnalyticsInsight
                {
                    Kind = InsightAdvice,
                    Title = "No active savings goals are defined",
                    Detected = "There are no goals with a target amount in the current finance context.",
                    WhyItMatters = "Goals turn abstract saving into visible progress and make it easier to protect money from impulse spending.",
                    SuggestedAction = "Create at least one near-term goal, even a small one, so the dashboard can measure progress against something concrete."
                });
            }
            else
            {
                decimal averageGoalProgress = Math.Round(activeGoals.Average(goal => Math.Min(goal.Percent, 100m)), 0);
                var strongestGoal = activeGoals.First();

                if (averageGoalProgress >= 70m)
                {
                    insights.Add(new AnalyticsInsight
                    {
                        Kind = InsightPraise,
                        Title = "Savings goals are moving well",
                        Detected = $"Active goals average {averageGoalProgress:0}% progress, led by {strongestGoal.GoalName} at {Math.Min(strongestGoal.Percent, 100m):0}%.",
                        WhyItMatters = "High visible progress makes it easier to stay consistent and harder to cannibalise savings for short-term wants.",
                        SuggestedAction = $"Keep feeding the strongest goal, then roll the same habit into the next incomplete target."
                    });
                }
                else
                {
                    var closestGoal = activeGoals
                        .OrderBy(goal => Math.Max(goal.Need - goal.Have, 0m))
                        .First();

                    insights.Add(new AnalyticsInsight
                    {
                        Kind = InsightAdvice,
                        Title = "Savings goals need stronger momentum",
                        Detected = $"Active goals average {averageGoalProgress:0}% progress; {closestGoal.GoalName} is currently the closest to completion.",
                        WhyItMatters = "Goals that move too slowly tend to lose attention, which makes saving feel optional again.",
                        SuggestedAction = $"Prioritise {closestGoal.GoalName} first and assign a fixed amount to it whenever new income lands."
                    });
                }
            }

            if (balance < 0m)
            {
                insights.Add(new AnalyticsInsight
                {
                    Kind = InsightWarning,
                    Title = "Balance is below zero",
                    Detected = $"The current balance is {balance:C0}.",
                    WhyItMatters = "A negative balance means future income is already committed to covering yesterday's spending.",
                    SuggestedAction = "Pause optional purchases, cover the shortfall first, and avoid moving more money into long-term goals until balance is back above zero."
                });
            }

            if (overruns.Count >= 2)
            {
                insights.Add(new AnalyticsInsight
                {
                    Kind = InsightWarning,
                    Title = "Overspending is spread across multiple categories",
                    Detected = $"{overruns.Count} categories are currently over their planned limit.",
                    WhyItMatters = "Multiple small overruns are harder to notice than one big spike, but together they create the same pressure.",
                    SuggestedAction = "Trim two overrun categories immediately rather than trying to fix everything everywhere at once."
                });
            }

            return insights
                .OrderBy(insight => GetInsightPriority(insight.Kind))
                .ThenBy(insight => insight.Title)
                .Take(6)
                .ToList();
        }

        private static List<AnalyticsMetric> BuildMetrics(
            IReadOnlyCollection<MonthlySpendTrendPoint> monthlyTrend,
            IReadOnlyCollection<SavingsGoal> goals,
            decimal savings,
            decimal averageMonthlyExpense,
            decimal averageGoalProgress,
            IReadOnlyCollection<AnalyticsInsight> insights,
            decimal cushionMonths)
        {
            decimal latestSpend = monthlyTrend.LastOrDefault()?.Amount ?? 0m;
            decimal trailingAverage = monthlyTrend.Count > 1 ? monthlyTrend.Take(monthlyTrend.Count - 1).TakeLast(3).Average(point => point.Amount) : 0m;
            decimal trendDelta = trailingAverage <= 0m ? 0m : ((latestSpend - trailingAverage) / trailingAverage) * 100m;

            int warningCount = insights.Count(insight => insight.Kind == InsightWarning);
            int adviceCount = insights.Count(insight => insight.Kind == InsightAdvice);
            int activeGoals = goals.Count(goal => goal.Need > 0m);

            return
            [
                new AnalyticsMetric
                {
                    Title = "Financial cushion",
                    Value = averageMonthlyExpense <= 0m ? (savings > 0m ? "Ready" : "No data") : $"{cushionMonths:0.0} months",
                    Caption = averageMonthlyExpense <= 0m
                        ? $"Savings currently sit at {savings:C0}."
                        : $"{savings:C0} saved against an average {averageMonthlyExpense:C0} monthly expense baseline."
                },
                new AnalyticsMetric
                {
                    Title = "Goals progress",
                    Value = activeGoals == 0 ? "No goals" : $"{averageGoalProgress:0}%",
                    Caption = activeGoals == 0
                        ? "Create a target to start tracking structured saving progress."
                        : $"{activeGoals} active goal(s) are currently being tracked."
                },
                new AnalyticsMetric
                {
                    Title = "Monthly trend",
                    Value = trailingAverage <= 0m ? "Starting point" : $"{trendDelta:+0;-0;0}%",
                    Caption = trailingAverage <= 0m
                        ? "Not enough history yet for a trend comparison."
                        : $"Latest month versus the recent 3-month average ({trailingAverage:C0})."
                },
                new AnalyticsMetric
                {
                    Title = "Risk level",
                    Value = warningCount >= 2 ? "High" : warningCount == 1 || adviceCount >= 2 ? "Guarded" : "Low",
                    Caption = $"{warningCount} warning(s) and {adviceCount} advice item(s) are active in the current analysis."
                }
            ];
        }
        #endregion

        #region Data Providers
        private async Task<List<SavingsGoal>> LoadGoalsAsync(bool usesHouseholdFinance, int? householdId)
        {
            if (usesHouseholdFinance)
            {
                if (!householdId.HasValue)
                {
                    return [];
                }

                return await _savingsGoalService.GetAllByHouseholdIdAsync(householdId.Value);
            }

            return await _savingsGoalService.GetAllByOwnerAsync();
        }

        private async Task<(decimal Savings, decimal Balance)> LoadFinanceAsync(bool usesHouseholdFinance, int userId, int? householdId)
        {
            if (usesHouseholdFinance)
            {
                var householdFinance = householdId.HasValue
                    ? await _householdFinanceService.GetHouseholdFinanceByHouseholdIdAsync(householdId.Value)
                    : null;

                return (householdFinance?.Savings ?? 0m, householdFinance?.Balance ?? 0m);
            }

            var userFinance = await _userFinanceService.GetByUserIdAsync(userId);
            return (userFinance?.Savings ?? 0m, userFinance?.Balance ?? 0m);
        }

        private async Task<List<MonthlySpendTrendPoint>> GetMonthlyTrendAsync(
            DateTime trendAnchorDate,
            CategoryFilterType categoryFilterType,
            int? householdId,
            int monthCount)
        {
            DateTime anchorMonth = new(trendAnchorDate.Year, trendAnchorDate.Month, 1);

            var monthStarts = Enumerable.Range(0, monthCount)
                .Select(index => anchorMonth.AddMonths(-(monthCount - 1 - index)))
                .ToList();

            var tasks = monthStarts.Select(async monthStart =>
            {
                DateTime monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                var records = await _recordService.GetRecordsByPeriodAsync(monthStart, monthEnd, categoryFilterType, householdId);
                decimal expenses = records
                    .Where(record => record.Type == RecordConstants.Types.Expense)
                    .Sum(record => record.Amount);

                return new MonthlySpendTrendPoint
                {
                    Label = monthStart.ToString("MMM", CultureInfo.CurrentCulture),
                    FullLabel = monthStart.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
                    Amount = expenses
                };
            });

            return [.. await Task.WhenAll(tasks)];
        }
        #endregion

        #region Helper Methods
        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated.");
            }

            return _userSessionService.CurrentUser!.Id;
        }

        private static decimal GetAverageGoalProgress(IReadOnlyCollection<SavingsGoal> goals)
        {
            var activeGoals = goals
                .Where(goal => goal.Need > 0m)
                .ToList();

            return activeGoals.Count == 0
                ? 0m
                : Math.Round(activeGoals.Average(goal => Math.Min(goal.Percent, 100m)), 0);
        }

        private static string BuildOverviewText(IReadOnlyCollection<AnalyticsInsight> insights, bool usesHouseholdFinance)
        {
            int warnings = insights.Count(insight => insight.Kind == InsightWarning);
            int praise = insights.Count(insight => insight.Kind == InsightPraise);
            string scope = usesHouseholdFinance ? "household" : "personal";

            if (warnings == 0 && praise == 0)
            {
                return $"Explainable analytics reviewed the current {scope} data and surfaced practical observations without any urgent red flags.";
            }

            return $"Explainable analytics reviewed the current {scope} data and found {warnings} warning(s) alongside {praise} positive signal(s).";
        }

        private static int GetInsightPriority(string kind)
        {
            return kind switch
            {
                InsightWarning => 0,
                InsightAdvice => 1,
                InsightObservation => 2,
                InsightPraise => 3,
                _ => 4
            };
        }
        #endregion
    }
}
