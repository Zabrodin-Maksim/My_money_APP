using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace My_money.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        #region Properties
        private decimal totalSpend;
        public decimal TotalSpend
        {
            get => totalSpend;
            set => SetProperty(ref totalSpend, value);
        }

        private decimal? balance;
        public decimal? Balance
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

        private IEnumerable<ISeries> expenseBreakdownSeries = Array.Empty<ISeries>();
        public IEnumerable<ISeries> ExpenseBreakdownSeries
        {
            get => expenseBreakdownSeries;
            set => SetProperty(ref expenseBreakdownSeries, value);
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

        private int selectedSortPeriod = 1; // 0 - day, 1 - month, 2 - year
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

        #region Commands
        public MyICommand<object> NavigateToAdd { get; }
        #endregion

        public DashboardViewModel(
            IBudgetCategoryService budgetCategoryService,
            IUserFinanceService userFinanceService,
            Services.NavigationService navigationService)
        {
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            _navigationService = navigationService;

            NavigateToAdd = new MyICommand<object>(NavigateToAddView);

            _ = LoadDataAsync();
        }

        #region Data Service Methods
        private async Task GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to)
        {
            BudgetCategories = new ObservableCollection<BudgetCategory>(
                await _budgetCategoryService.GetAllBudgetCategoriesByPeriodAsync(from, to));
        }

        private Task<UserFinance> GetUserFinanceAsync()
        {
            return _userFinanceService.GetUserFinanceAsync();
        }
        #endregion

        private async Task LoadDataAsync()
        {
            await RefreshDashboardAsync();
        }

        private async Task RefreshDashboardAsync()
        {
            var (from, to) = GetPeriodRange(SelectedDate, SelectedSortPeriod);
            SelectedPeriodLabel = BuildPeriodLabel(from, to, SelectedSortPeriod);

            await GetAllBudgetCategoriesByPeriodAsync(from, to);

            TotalSpend = BudgetCategories.Sum(cat => cat.SpendByPeriod ?? 0m);

            var userFinance = await GetUserFinanceAsync();
            Balance = userFinance.Balance ?? 0m;
            Savings = userFinance.Savings ?? 0m;

            UpdateBudgetStatus();
        }

        private void UpdateBudgetStatus()
        {
            var totalPlanned = BudgetCategories.Sum(cat => cat.PlanByPeriod ?? cat.Plan ?? 0m);

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
    }
}
