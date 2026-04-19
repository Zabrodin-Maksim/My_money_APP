using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace My_money.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IHouseholdFinanceService _householdFinanceService;
        private readonly IUserSessionService _userSessionService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public DashboardViewModel(
            IBudgetCategoryService budgetCategoryService,
            IUserFinanceService userFinanceService,
            IHouseholdFinanceService householdFinanceService,
            IUserSessionService userSessionService,
            Services.NavigationService navigationService)
        {
            #region Dependency Injection
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            _householdFinanceService = householdFinanceService;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            #endregion

            NavigateToAdd = new MyICommand<object>(NavigateToAddView);

            InitializeContexts();
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

            UpdateBudgetStatus();
            UpdateContextDescription();
        }

        private void UpdateBudgetStatus()
        {
            var totalPlanned = BudgetCategories.Sum(cat => cat.PlanByPeriod ?? cat.Plan);

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
