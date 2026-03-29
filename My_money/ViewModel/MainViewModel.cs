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
        private readonly Services.NavigationService _navigationService;
        #endregion

        public MainViewModel(Services.NavigationService navigationService)
        {
            _navigationService = navigationService;

            #region Commands
            NavigateToDashboard = new MyICommand<object>(NavigateToDashboardView);
            NavigateToHistory = new MyICommand<object>(NavigateToHistoryView);
            NavigateToPlan = new MyICommand<object>(NavigateToPlanView);
            NavigateToMoneyBox = new MyICommand<object>(NavigateToBoxView);

            ExitCommand = new MyICommand<object>(OnExit);
            MinimizeWindowCommand = new MyICommand<object>(MinimizeWindow);
            //MaximizeWindowCommand = new MyICommand<object>(MaximizeWindow);
            #endregion
        }

        #region Commands

        #region Navigation Commands
        public MyICommand<object> NavigateToDashboard { get; private set; }
        public MyICommand<object> NavigateToHistory { get; private set; }
        public MyICommand<object> NavigateToPlan { get; private set; }
        public MyICommand<object> NavigateToMoneyBox { get; private set; }

        #endregion

        public MyICommand<object> ExitCommand { get; private set; }

        public MyICommand<object> MinimizeWindowCommand { get; private set; }

        //public MyICommand<object> MaximizeWindowCommand { get; private set; }
        #endregion

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
