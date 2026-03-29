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
        private readonly Services.NavigationService _navigationService;
        #endregion

        #region Properties
        private decimal totalSpend;
        public decimal TotalSpend { get { return totalSpend; } set { SetProperty(ref totalSpend, value); } }

        private decimal? balance;
        public decimal? Balance { get { return balance; } set { SetProperty(ref balance, value); } }

        private decimal savings;
        public decimal Savings { get { return savings; } set { SetProperty(ref savings, value); } }


        private ObservableCollection<BudgetCategory> budgetCategories;
        public ObservableCollection<BudgetCategory> BudgetCategories { get { return budgetCategories; } set { SetProperty(ref budgetCategories, value); ; } }

        #region Period for sorting
        private int selectedSortPeriod = 1; //0 - day, 1 - month, 2 - year
        public int SelectedSortPeriod
        {
            get { return selectedSortPeriod; }
            set
            {
                selectedSortPeriod = value;
                _ = RefreshPeriod();
            }
        }

        private DateTime selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                _ = RefreshPeriod();
            }
        }
        #endregion

        #endregion

        #region Commands
        public MyICommand<object> NavigateToAdd { get; private set; }
        #endregion

        public DashboardViewModel(IBudgetCategoryService budgetCategoryService, IUserFinanceService userFinanceService, Services.NavigationService navigationService) 
        {
            #region DI Services
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            _navigationService = navigationService;
            #endregion

            NavigateToAdd = new MyICommand<object>(NavigateToAddView);

            _ = LoadDataAsync();
        }

        #region Data Service Methods

        #region Budget Category
        private async Task GetAllBudgetCategoriesByPeriodAsync(DateTime from, DateTime to)
        {
            BudgetCategories = new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllBudgetCategoriesByPeriodAsync(from, to));
        }

        #endregion

        #region User Finance
        private async Task<UserFinance> GetUserFinanceAsync()
        {
            return await _userFinanceService.GetUserFinanceAsync();
        }
        #endregion

        #endregion

        private async Task LoadDataAsync()
        {
            // TODO: UI ТУТ МОЖНО БУДЕТ ДОБАВИТЬ ТИПА ЗАГРУЗКУ ДАННЫХ С ПРОГРЕСС БАРОМ
            await RefreshPeriod();
            TotalSpend = BudgetCategories.Count > 0 ? BudgetCategories.Sum(cat => cat.SpendByPeriod) ?? 0 : 0;
            var userFinance = await GetUserFinanceAsync();
            Balance = userFinance.Balance ?? 0;
            Savings = userFinance.Savings ?? 0;
        }

        private async Task RefreshPeriod()
        {
            var (from, to) = GetPeriodRange(SelectedDate, SelectedSortPeriod);
            await GetAllBudgetCategoriesByPeriodAsync(from, to);
        }

        private (DateTime from, DateTime to) GetPeriodRange(DateTime selectedDate, int selectedSortPeriod)
        {
            DateTime from, to;

            switch (selectedSortPeriod)
            {
                case 0: // day
                    from = selectedDate.Date;
                    to = selectedDate.Date.AddDays(1).AddTicks(-1);
                    break;

                case 1: // mounth
                    from = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                    to = from.AddMonths(1).AddTicks(-1);
                    break;

                case 2: // year
                    from = new DateTime(selectedDate.Year, 1, 1);
                    to = new DateTime(selectedDate.Year + 1, 1, 1).AddTicks(-1);
                    break;

                default:
                    from = selectedDate.Date;
                    to = selectedDate.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            return (from, to);
        }

        private async Task NavigateToAddView(object o)
        {
            _navigationService.Navigate(ViewID.AddView);
        }
    }
}
