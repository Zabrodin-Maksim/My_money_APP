using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace My_money.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserFinanceService _userFinanceService;
        #endregion
        /* Notes
         * 
         * !. Вместо акций и всей остальной херни (типа связи между VM для пресчета данных),
         * при переходе на dashboard просто брать из db все по новой
         * (т.е. сделать метод на обновление данных)
         * 
         * !. 
         */


        public MainViewModel(
            IBudgetCategoryService budgetCategoryService,
            IUserFinanceService userFinanceService)
        {
            //Start View
            CurrentView = dashboardView;

            #region DI Services
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            #endregion

            // Initialize data
            UpdateDashboardData();

            #region Commands
            NavCommand = new MyICommand<string>(OnNav);

            ExitCommand = new MyICommand<object>(OnExit);
            MinimizeWindowCommand = new MyICommand<object>(MinimizeWindow);
            //MaximizeWindowCommand = new MyICommand<object>(MaximizeWindow);
            #endregion

            addViewModel.Back += OnNav;

        }


        #region Properties
        private decimal totalSpend;
        public decimal TotalSpend { get { return totalSpend; } set { SetProperty(ref totalSpend, value); } }

        private decimal balance;
        public decimal Balance { get { return balance; } set { SetProperty(ref balance, value); } }

        private decimal savings;
        public decimal Savings { get { return savings; } set { SetProperty(ref savings, value); } }


        private ObservableCollection<BudgetCategory> budgetCategories;
        public ObservableCollection<BudgetCategory> BudgetCategories { get { return budgetCategories; } set { budgetCategories = value; } }

        #region Period for sorting
        private int selectedSortPeriod = 1; //0 - day, 1 - month, 2 - year
        public int SelectedSortPeriod
        {
            get { return selectedSortPeriod; }
            set
            {
                selectedSortPeriod = value;
                RefreshPeriod();
            }
        }

        private DateTime selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                RefreshPeriod();
            }
        }
        #endregion

        #endregion


        #region Commands
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand<object> ExitCommand { get; private set; }

        public MyICommand<object> MinimizeWindowCommand { get; private set; }

        //public MyICommand<object> MaximizeWindowCommand { get; private set; }
        #endregion


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

        private async void UpdateDashboardData()
        {
            // TODO: ТУТ МОЖНО БУДЕТ ДОБАВИТЬ ТИПА ЗАГРУЗКУ ДАННЫХ С ПРОГРЕСС БАРОМ
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

        #region NAVIGATION

        private UserControl currentView;
        public UserControl CurrentView
        {
            get { return currentView; }
            set
            {
                SetProperty(ref currentView, value);
            }
        }

        #region Views
        private DashboardView dashboardView = new DashboardView();
        private AddView addView = new AddView();
        private HistoryView historyView = new HistoryView();
        private PlanView planView = new PlanView();
        private MoneyBoxView moneyBoxView = new MoneyBoxView();
        #endregion

        #region ViewModel
        private AddViewModel addViewModel;
        private HistoryViewModel historyViewModel;
        private PlanViewModel planViewModel;
        private MoneyBoxViewModel moneyBoxViewModel;
        #endregion

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    CurrentView = dashboardView;
                    break;

                case "AddRecord":
                    CurrentView = addView;
                    addView.DataContext = addViewModel;
                    break;

                case "History":
                    CurrentView = historyView;
                    historyView.DataContext = historyViewModel;
                    historyViewModel.SortingRecords();
                    break;

                case "Plan":
                    CurrentView = planView;
                    planView.DataContext = planViewModel;
                    break;

                case "Moneybox":
                    CurrentView = moneyBoxView;
                    moneyBoxView.DataContext = moneyBoxViewModel;
                    break;
            }
        }
        #endregion


        #region Exit
        private void OnExit(object param)
        {
            Application.Current.Shutdown();
        }
        #endregion


        #region Minimize Window
        private void MinimizeWindow(object par)
        {
            SystemCommands.MinimizeWindow(Application.Current.MainWindow);
        }
        #endregion


        //#region Maximize Window()
        //private void MaximizeWindow(object par)
        //{
        //    SystemCommands.MaximizeWindow(Application.Current.MainWindow);
        //}
        //#endregion
    }
}
