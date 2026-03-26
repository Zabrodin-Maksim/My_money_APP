using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly Services.NavigationService _navigationService;
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
            IUserFinanceService userFinanceService,
            Services.NavigationService navigationService
            )
        {

            #region DI Services
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            _navigationService = navigationService;
            #endregion

            // Initialize data
            _ = LoadDataAsync();

            #region Commands
            NavigateToDashboard = new MyICommand<object>(NavigateToDashboardView);
            NavigateToAdd = new MyICommand<object>(NavigateToAddView);
            NavigateToHistory = new MyICommand<object>(NavigateToHistoryView);
            NavigateToPlan = new MyICommand<object>(NavigateToPlanView);
            NavigateToMoneyBox = new MyICommand<object>(NavigateToBoxView);

            ExitCommand = new MyICommand<object>(OnExit);
            MinimizeWindowCommand = new MyICommand<object>(MinimizeWindow);
            //MaximizeWindowCommand = new MyICommand<object>(MaximizeWindow);
            #endregion

        }

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

        #region Navigation Commands
        public MyICommand<object> NavigateToDashboard { get; private set; }
        public MyICommand<object> NavigateToAdd { get; private set; }
        public MyICommand<object> NavigateToHistory { get; private set; }
        public MyICommand<object> NavigateToPlan { get; private set; }
        public MyICommand<object> NavigateToMoneyBox { get; private set; }

        #endregion

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

        // Узел DI, внутри NavigationService происходит инъекция нужной VM в зависимости от переданного ViewID
        #region NAVIGATION

        private ViewModelBase currentView;
        public ViewModelBase CurrentView
        {
            get { return currentView; }
            set { SetProperty(ref currentView, value); }
        }

        private async Task NavigateToDashboardView(object o)
        {
            _navigationService.Navigate(ViewID.DashboardView);
        }

        private async Task NavigateToAddView(object o)
        {
            _navigationService.Navigate(ViewID.AddView);
        }

        private async Task NavigateToHistoryView(object o)
        {
            _navigationService.Navigate(ViewID.HistoryView);
        }

        private async Task NavigateToPlanView(object o)
        {
            _navigationService.Navigate(ViewID.PlanView);
        }

        private async Task NavigateToBoxView(object o)
        {
            _navigationService.Navigate(ViewID.MoneyBoxView);
        }
        #endregion


        #region Exit
        private Task OnExit(object param)
        {
            Application.Current.Shutdown();
            return Task.CompletedTask;
        }
        #endregion


        #region Minimize Window
        private Task MinimizeWindow(object par)
        {
            SystemCommands.MinimizeWindow(Application.Current.MainWindow);
            return Task.CompletedTask;
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
