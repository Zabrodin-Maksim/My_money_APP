using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace My_money.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        private static readonly SKColor[] ChartPalette =
        [
            new(79, 140, 255),
            new(52, 211, 196),
            new(247, 178, 103),
            new(248, 113, 113),
            new(96, 165, 250),
            new(45, 212, 191)
        ];
        private static readonly SKColor PressureSafeColor = new(82, 199, 140);
        private static readonly SKColor PressureWatchColor = new(240, 181, 106);
        private static readonly SKColor PressureDangerColor = new(255, 125, 125);

        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IHouseholdFinanceService _householdFinanceService;
        private readonly IExplainableAnalyticsService _explainableAnalyticsService;
        private readonly IUserSessionService _userSessionService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public DashboardViewModel(
            IBudgetCategoryService budgetCategoryService,
            IUserFinanceService userFinanceService,
            IHouseholdFinanceService householdFinanceService,
            IExplainableAnalyticsService explainableAnalyticsService,
            IUserSessionService userSessionService,
            Services.NavigationService navigationService)
        {
            #region Dependency Injection
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            _householdFinanceService = householdFinanceService;
            _explainableAnalyticsService = explainableAnalyticsService;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            #endregion

            NavigateToAdd = new MyICommand<object>(NavigateToAddView);

            InitializeContexts();
            InitializeCharts();
            _ = RefreshDashboardAsync();
        }

        public MyICommand<object> NavigateToAdd { get; }

        #region Properties
        private ObservableCollection<ContextOption> availableContexts = new();
        public ObservableCollection<ContextOption> AvailableContexts
        {
            get => availableContexts;
            set => SetProperty(ref availableContexts, value);
        }

        private ContextOption selectedContext;
        public ContextOption SelectedContext
        {
            get => selectedContext;
            set
            {
                if (selectedContext == value || value is null)
                {
                    return;
                }

                SetProperty(ref selectedContext, value);
                OnPropertyChanged(nameof(BalanceCaption));
                OnPropertyChanged(nameof(SavingsCaption));
                _ = RefreshDashboardAsync();
            }
        }


        private decimal totalSpend;
        public decimal TotalSpend
        {
            get => totalSpend;
            set => SetProperty(ref totalSpend, value);
        }

        private decimal balance;
        public decimal Balance
        {
            get => balance;
            set => SetProperty(ref balance, value);
        }

        private decimal savings;
        public decimal Savings
        {
            get => savings;
            set => SetProperty(ref savings, value);
        }

        private ObservableCollection<BudgetCategory> budgetCategories = new();
        public ObservableCollection<BudgetCategory> BudgetCategories
        {
            get => budgetCategories;
            set => SetProperty(ref budgetCategories, value);
        }

        private string selectedPeriodLabel = string.Empty;
        public string SelectedPeriodLabel
        {
            get => selectedPeriodLabel;
            set => SetProperty(ref selectedPeriodLabel, value);
        }

        private string budgetStatusText = "Overview";
        public string BudgetStatusText
        {
            get => budgetStatusText;
            set => SetProperty(ref budgetStatusText, value);
        }

        private string contextDescription = string.Empty;
        public string ContextDescription
        {
            get => contextDescription;
            set => SetProperty(ref contextDescription, value);
        }

        private ISeries[] spendDistributionSeries = [];
        public ISeries[] SpendDistributionSeries
        {
            get => spendDistributionSeries;
            set => SetProperty(ref spendDistributionSeries, value);
        }

        private ISeries[] budgetPressureSeries = [];
        public ISeries[] BudgetPressureSeries
        {
            get => budgetPressureSeries;
            set => SetProperty(ref budgetPressureSeries, value);
        }

        private Axis[] pressureXAxes = [];
        public Axis[] PressureXAxes
        {
            get => pressureXAxes;
            set => SetProperty(ref pressureXAxes, value);
        }

        private Axis[] pressureYAxes = [];
        public Axis[] PressureYAxes
        {
            get => pressureYAxes;
            set => SetProperty(ref pressureYAxes, value);
        }

        private string chartInsight = "Add categories and transactions to unlock the chart layer.";
        public string ChartInsight
        {
            get => chartInsight;
            set => SetProperty(ref chartInsight, value);
        }

        private string budgetProgressLabel = "No active budget plan";
        public string BudgetProgressLabel
        {
            get => budgetProgressLabel;
            set => SetProperty(ref budgetProgressLabel, value);
        }

        private string budgetPressureInsight = "Planned categories will appear here once the dashboard has enough budget data to compare.";
        public string BudgetPressureInsight
        {
            get => budgetPressureInsight;
            set => SetProperty(ref budgetPressureInsight, value);
        }

        public ObservableCollection<AnalyticsMetric> AnalyticsMetrics { get; } = new();
        public ObservableCollection<AnalyticsInsight> AnalyticsInsights { get; } = new();

        private string analyticsOverview = "Explainable analytics turns tracked finance data into transparent observations and actions.";
        public string AnalyticsOverview
        {
            get => analyticsOverview;
            set => SetProperty(ref analyticsOverview, value);
        }

        private ISeries[] monthlySpendTrendSeries = [];
        public ISeries[] MonthlySpendTrendSeries
        {
            get => monthlySpendTrendSeries;
            set => SetProperty(ref monthlySpendTrendSeries, value);
        }

        private Axis[] trendXAxes = [];
        public Axis[] TrendXAxes
        {
            get => trendXAxes;
            set => SetProperty(ref trendXAxes, value);
        }

        private Axis[] trendYAxes = [];
        public Axis[] TrendYAxes
        {
            get => trendYAxes;
            set => SetProperty(ref trendYAxes, value);
        }

        private int selectedSortPeriod = 1;
        public int SelectedSortPeriod
        {
            get => selectedSortPeriod;
            set
            {
                if (selectedSortPeriod == value)
                {
                    return;
                }

                selectedSortPeriod = value;
                OnPropertyChanged(nameof(SelectedSortPeriod));
                _ = RefreshDashboardAsync();
            }
        }

        private DateTime selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => selectedDate;
            set
            {
                if (selectedDate == value)
                {
                    return;
                }

                selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                _ = RefreshDashboardAsync();
            }
        }
        #endregion

        #region Computed Properties
        public string BalanceCaption => SelectedContext?.UsesHouseholdFinance == true ? "Household balance" : "Personal balance";
        public string SavingsCaption => SelectedContext?.UsesHouseholdFinance == true ? "Household savings" : "Personal savings";

        private int AuthenticatedUserId => _userSessionService.CurrentUser?.Id ?? 0;
        private int? HouseholdId => _userSessionService.CurrentHouseholdMember?.HouseholdId;
        private bool IsChild => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child);
        #endregion

        private void InitializeContexts()
        {
            AvailableContexts.Clear();
            AvailableContexts.Add(new ContextOption
            {
                Title = "Household",
                FilterType = CategoryFilterType.Household,
                UsesHouseholdFinance = true
            });

            if (!IsChild)
            {
                AvailableContexts.Add(new ContextOption
                {
                    Title = "Personal",
                    FilterType = CategoryFilterType.Personal,
                    UsesHouseholdFinance = false
                });
            }
            else
            {
                AvailableContexts.Add(new ContextOption
                {
                    Title = "My shared activity",
                    FilterType = CategoryFilterType.Child,
                    UsesHouseholdFinance = true
                });
            }

            SelectedContext = AvailableContexts.First();
        }

        private async Task RefreshDashboardAsync()
        {
            if (SelectedContext is null)
            {
                return;
            }

            var (from, to) = GetPeriodRange(SelectedDate, SelectedSortPeriod);
            var analyticsTask = _explainableAnalyticsService.BuildDashboardAnalyticsAsync(
                from,
                to,
                SelectedDate,
                SelectedContext.FilterType,
                SelectedContext.UsesHouseholdFinance,
                HouseholdId);

            SelectedPeriodLabel = BuildPeriodLabel(from, to, SelectedSortPeriod);

            BudgetCategories = new ObservableCollection<BudgetCategory>(
                await _budgetCategoryService.GetAllBudgetCategoriesByPeriodAsync(from, to, SelectedContext.FilterType, HouseholdId));

            TotalSpend = BudgetCategories.Sum(cat => cat.SpendByPeriod ?? 0m);

            if (SelectedContext.UsesHouseholdFinance)
            {
                var householdFinance = HouseholdId.HasValue
                    ? await _householdFinanceService.GetHouseholdFinanceByHouseholdIdAsync(HouseholdId.Value)
                    : null;

                Balance = householdFinance?.Balance ?? 0m;
                Savings = householdFinance?.Savings ?? 0m;
            }
            else
            {
                var userFinance = await _userFinanceService.GetByUserIdAsync(AuthenticatedUserId);
                Balance = userFinance?.Balance ?? 0m;
                Savings = userFinance?.Savings ?? 0m;
            }

            decimal totalPlanned = BudgetCategories.Sum(cat => cat.PlanByPeriod ?? cat.Plan);

            UpdateBudgetStatus(totalPlanned);
            UpdateContextDescription();
            UpdateCharts(totalPlanned);
            ApplyAnalyticsSnapshot(await analyticsTask);
        }

        #region Chart Initialization
        private void InitializeCharts()
        {
            var textPaint = new SolidColorPaint(new SKColor(243, 246, 255));
            var separatorPaint = new SolidColorPaint(new SKColor(120, 132, 165, 90), 1);

            PressureXAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 140,
                    MinStep = 20,
                    LabelsPaint = textPaint,
                    SeparatorsPaint = separatorPaint,
                    Labeler = value => $"{value:0}%"
                }
            ];

            PressureYAxes =
            [
                new Axis
                {
                    Labels = [],
                    LabelsPaint = textPaint,
                    SeparatorsPaint = null
                }
            ];

            TrendYAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    LabelsPaint = textPaint,
                    SeparatorsPaint = separatorPaint,
                    Labeler = value => value.ToString("C0")
                }
            ];

            TrendXAxes =
            [
                new Axis
                {
                    Labels = [],
                    LabelsPaint = textPaint,
                    SeparatorsPaint = null
                }
            ];
        }
        #endregion

        #region View State Updates
        private void UpdateBudgetStatus(decimal totalPlanned)
        {
            if (totalPlanned <= 0m)
            {
                BudgetStatusText = "No budget target";
                return;
            }

            var usage = TotalSpend / totalPlanned;

            if (usage < 0.65m)
            {
                BudgetStatusText = "Comfortable pace";
            }
            else if (usage <= 1m)
            {
                BudgetStatusText = "Close to plan";
            }
            else
            {
                BudgetStatusText = "Over plan";
            }
        }

        private void UpdateContextDescription()
        {
            ContextDescription = SelectedContext.FilterType switch
            {
                CategoryFilterType.Household => "Shared household categories and spending for the selected period.",
                CategoryFilterType.Personal => "Private categories and spending that belong only to the signed-in user.",
                CategoryFilterType.Child when IsChild => "Your own shared contributions inside the household budget.",
                _ => "Finance context overview."
            };
        }

        private void UpdateCharts(decimal totalPlanned)
        {
            var rankedCategories = BudgetCategories
                .Where(cat => (cat.SpendByPeriod ?? 0m) > 0m || (cat.PlanByPeriod ?? cat.Plan) > 0m)
                .OrderByDescending(cat => cat.SpendByPeriod ?? 0m)
                .ToList();

            UpdateSpendDistributionChart(rankedCategories);
            UpdateBudgetPressureChart(rankedCategories);
            UpdateBudgetProgressSummary(totalPlanned);
            UpdateChartInsight(rankedCategories, totalPlanned);
        }

        private void UpdateSpendDistributionChart(System.Collections.Generic.IReadOnlyList<BudgetCategory> rankedCategories)
        {
            var spendCategories = rankedCategories
                .Where(cat => (cat.SpendByPeriod ?? 0m) > 0m)
                .OrderByDescending(cat => cat.SpendByPeriod ?? 0m)
                .ToList();

            if (spendCategories.Count == 0)
            {
                SpendDistributionSeries =
                [
                    new PieSeries<double>
                    {
                        Name = "No spending yet",
                        Values = [1],
                        DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                        InnerRadius = 58,
                        Fill = new SolidColorPaint(new SKColor(226, 232, 240)),
                        Stroke = new SolidColorPaint(SKColors.White, 3)
                    }
                ];
                return;
            }

            var topCategories = spendCategories.Take(5).ToList();
            decimal otherSpend = spendCategories.Skip(5).Sum(cat => cat.SpendByPeriod ?? 0m);

            var series = topCategories
                .Select((cat, index) => (ISeries)new PieSeries<double>
                {
                    Name = cat.Name,
                    Values = [Convert.ToDouble(cat.SpendByPeriod ?? 0m)],
                    DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    InnerRadius = 58,
                    Fill = new SolidColorPaint(ChartPalette[index % ChartPalette.Length]),
                    Stroke = new SolidColorPaint(SKColors.White, 3)
                })
                .ToList();

            if (otherSpend > 0m)
            {
                series.Add(new PieSeries<double>
                {
                    Name = "Other",
                    Values = [Convert.ToDouble(otherSpend)],
                    DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    InnerRadius = 58,
                    Fill = new SolidColorPaint(new SKColor(148, 163, 184)),
                    Stroke = new SolidColorPaint(SKColors.White, 3)
                });
            }

            SpendDistributionSeries = [.. series];
        }

        private void UpdateBudgetPressureChart(System.Collections.Generic.IReadOnlyList<BudgetCategory> rankedCategories)
        {
            var pressureCategories = rankedCategories
                .Where(cat => (cat.PlanByPeriod ?? cat.Plan) > 0m)
                .OrderByDescending(CalculateBudgetPressure)
                .Take(6)
                .ToList();

            if (pressureCategories.Count == 0)
            {
                BudgetPressureSeries =
                [
                    new RowSeries<BudgetPressurePoint>
                    {
                        Name = "Budget pressure",
                        Values =
                        [
                            new BudgetPressurePoint("No planned categories", 0, "0%", new SolidColorPaint(ChartPalette[0]))
                        ],
                        DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                        DataLabelsPosition = DataLabelsPosition.End,
                        DataLabelsFormatter = point => ((BudgetPressurePoint)point.Model!).Label,
                        MaxBarWidth = 20,
                        Padding = 8
                    }
                    .OnPointMeasured(point =>
                    {
                        if (point.Visual is null)
                        {
                            return;
                        }

                        point.Visual.Fill = ((BudgetPressurePoint)point.Model!).Paint;
                    })
                ];

                PressureYAxes =
                [
                    new Axis
                    {
                        Labels = ["No planned categories"],
                        LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                        SeparatorsPaint = null
                    }
                ];

                BudgetPressureInsight = "No categories with an active plan were found for the current range yet.";
                return;
            }

            var pressurePoints = pressureCategories
                .Select(category =>
                {
                    decimal pressure = CalculateBudgetPressure(category);
                    return new BudgetPressurePoint(
                        category.Name,
                        Convert.ToDouble(pressure),
                        $"{pressure:0}%",
                        new SolidColorPaint(GetPressureColor(pressure)));
                })
                .Reverse()
                .ToArray();

            double maxPointValue = pressurePoints.Max(point => point.Value ?? 0d);
            double maxPressure = Math.Max(120d, Math.Ceiling(maxPointValue / 20d) * 20d);

            BudgetPressureSeries =
            [
                new RowSeries<BudgetPressurePoint>
                {
                    Name = "Budget pressure",
                    Values = [.. pressurePoints],
                    DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    DataLabelsPosition = DataLabelsPosition.End,
                    DataLabelsFormatter = point => ((BudgetPressurePoint)point.Model!).Label,
                    MaxBarWidth = 20,
                    Padding = 8
                }
                .OnPointMeasured(point =>
                {
                    if (point.Visual is null)
                    {
                        return;
                    }

                    point.Visual.Fill = ((BudgetPressurePoint)point.Model!).Paint;
                })
            ];

            PressureXAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = maxPressure,
                    MinStep = 20,
                    LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    SeparatorsPaint = new SolidColorPaint(new SKColor(120, 132, 165, 90), 1),
                    Labeler = value => $"{value:0}%"
                }
            ];

            PressureYAxes =
            [
                new Axis
                {
                    Labels = [.. pressurePoints.Select(point => point.Name)],
                    LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    SeparatorsPaint = null
                }
            ];

            var mostPressuredCategory = pressureCategories.First();
            decimal mostPressuredValue = CalculateBudgetPressure(mostPressuredCategory);

            BudgetPressureInsight = mostPressuredValue > 100m
                ? $"{mostPressuredCategory.Name} is already above plan at {mostPressuredValue:0}% usage, so this category should be reviewed first."
                : mostPressuredValue >= 85m
                    ? $"{mostPressuredCategory.Name} is the closest category to its limit at {mostPressuredValue:0}% usage."
                    : "All planned categories are still below 85% of their current budget pace.";
        }

        private void UpdateBudgetProgressSummary(decimal totalPlanned)
        {
            if (totalPlanned <= 0m)
            {
                BudgetProgressLabel = "No active budget plan";
                return;
            }

            if (TotalSpend <= totalPlanned)
            {
                decimal remaining = totalPlanned - TotalSpend;
                decimal usage = totalPlanned == 0m ? 0m : Math.Round((TotalSpend / totalPlanned) * 100m, 0);

                BudgetProgressLabel = $"{usage}% of plan used, {remaining:C0} left";

                return;
            }

            decimal overrun = TotalSpend - totalPlanned;
            decimal overrunPct = totalPlanned == 0m ? 0m : Math.Round((overrun / totalPlanned) * 100m, 0);

            BudgetProgressLabel = $"{overrunPct}% over plan, {overrun:C0} above target";
        }

        private void UpdateChartInsight(System.Collections.Generic.IReadOnlyList<BudgetCategory> rankedCategories, decimal totalPlanned)
        {
            if (rankedCategories.Count == 0)
            {
                ChartInsight = "No budget categories matched the current filters yet. Once spending lands here, the dashboard will spotlight what drives the period.";
                return;
            }

            var topSpendCategory = rankedCategories
                .OrderByDescending(cat => cat.SpendByPeriod ?? 0m)
                .FirstOrDefault();

            if (topSpendCategory is null || (topSpendCategory.SpendByPeriod ?? 0m) <= 0m || TotalSpend <= 0m)
            {
                ChartInsight = "Categories are ready, but there is no spend in the selected range yet. Use the add form to start building trends.";
                return;
            }

            decimal topSpend = topSpendCategory.SpendByPeriod ?? 0m;
            decimal share = TotalSpend == 0m ? 0m : Math.Round((topSpend / TotalSpend) * 100m, 0);
            decimal delta = totalPlanned - TotalSpend;

            ChartInsight = delta >= 0m
                ? $"{topSpendCategory.Name} drives {share}% of the current spend. The dashboard still has {delta:C0} of budget headroom in this period."
                : $"{topSpendCategory.Name} drives {share}% of the current spend. The dashboard is currently {-delta:C0} over the planned budget.";
        }

        private void ApplyAnalyticsSnapshot(DashboardAnalyticsSnapshot snapshot)
        {
            AnalyticsOverview = snapshot.OverviewText;

            AnalyticsMetrics.Clear();
            foreach (var metric in snapshot.Metrics)
            {
                AnalyticsMetrics.Add(metric);
            }

            AnalyticsInsights.Clear();
            foreach (var insight in snapshot.Insights)
            {
                AnalyticsInsights.Add(insight);
            }

            UpdateMonthlyTrendChart(snapshot.MonthlyTrend);
        }

        private void UpdateMonthlyTrendChart(System.Collections.Generic.IReadOnlyList<MonthlySpendTrendPoint> monthlyTrend)
        {
            if (monthlyTrend.Count == 0)
            {
                MonthlySpendTrendSeries =
                [
                    new LineSeries<double>
                    {
                        Name = "Spend",
                        Values = [0],
                        Stroke = new SolidColorPaint(ChartPalette[0], 3),
                        Fill = null
                    }
                ];

                TrendXAxes =
                [
                    new Axis
                    {
                        Labels = ["No data"],
                        LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                        SeparatorsPaint = null
                    }
                ];

                return;
            }

            MonthlySpendTrendSeries =
            [
                new LineSeries<double>
                {
                    Name = "Monthly spend",
                    Values = [.. monthlyTrend.Select(point => Convert.ToDouble(point.Amount))],
                    Stroke = new SolidColorPaint(ChartPalette[0], 3),
                    Fill = new SolidColorPaint(new SKColor(124, 147, 255, 50)),
                    GeometrySize = 10,
                    GeometryFill = new SolidColorPaint(ChartPalette[1]),
                    GeometryStroke = new SolidColorPaint(new SKColor(243, 246, 255), 2)
                }
            ];

            TrendXAxes =
            [
                new Axis
                {
                    Labels = [.. monthlyTrend.Select(point => point.Label)],
                    LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    SeparatorsPaint = null
                }
            ];
        }
        #endregion

        #region Helpers
        private static decimal CalculateBudgetPressure(BudgetCategory category)
        {
            decimal planned = category.PlanByPeriod ?? category.Plan;
            decimal spent = category.SpendByPeriod ?? 0m;

            if (planned <= 0m)
            {
                return 0m;
            }

            return Math.Round((spent / planned) * 100m, 0);
        }

        private static SKColor GetPressureColor(decimal pressure)
        {
            if (pressure > 100m)
            {
                return PressureDangerColor;
            }

            if (pressure >= 70m)
            {
                return PressureWatchColor;
            }

            return PressureSafeColor;
        }

        private static string BuildPeriodLabel(DateTime from, DateTime to, int selectedSortPeriod)
        {
            return selectedSortPeriod switch
            {
                0 => from.ToString("dd MMMM yyyy"),
                1 => from.ToString("MMMM yyyy"),
                2 => from.ToString("yyyy"),
                _ => $"{from:dd MMM yyyy} - {to:dd MMM yyyy}"
            };
        }

        private static (DateTime from, DateTime to) GetPeriodRange(DateTime date, int period)
        {
            return period switch
            {
                0 => (date.Date, date.Date.AddDays(1).AddTicks(-1)),
                1 => (new DateTime(date.Year, date.Month, 1), new DateTime(date.Year, date.Month, 1).AddMonths(1).AddTicks(-1)),
                2 => (new DateTime(date.Year, 1, 1), new DateTime(date.Year + 1, 1, 1).AddTicks(-1)),
                _ => (date.Date, date.Date.AddDays(1).AddTicks(-1))
            };
        }

        private Task NavigateToAddView(object _)
        {
            _navigationService.Navigate(ViewID.AddView);
            return Task.CompletedTask;
        }
        #endregion

        #region Chart Models
        private sealed class BudgetPressurePoint : ObservableValue
        {
            public BudgetPressurePoint(string name, double value, string label, SolidColorPaint paint)
            {
                Name = name;
                Value = value;
                Label = label;
                Paint = paint;
            }

            public string Name { get; }
            public string Label { get; }
            public SolidColorPaint Paint { get; }
        }
        #endregion
    }
}
