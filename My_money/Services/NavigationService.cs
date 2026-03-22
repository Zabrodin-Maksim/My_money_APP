using My_money.ViewModel;
using My_money.Views;

namespace My_money.Services
{
    public class NavigationService
    {
        private readonly MainViewModel _mainViewModel;
        private readonly AddViewModel _addViewModel;
        private readonly HistoryViewModel _historyViewModel;
        private readonly PlanViewModel _planViewModel;
        private readonly MoneyBoxViewModel _moneyBoxViewModel;

        public NavigationService(MainViewModel mainViewModel,
                                 AddViewModel addViewModel,
                                 HistoryViewModel historyViewModel,
                                 PlanViewModel planViewModel,
                                 MoneyBoxViewModel moneyBoxViewModel)
        {
            _mainViewModel = mainViewModel;
            _addViewModel = addViewModel;
            _historyViewModel = historyViewModel;
            _planViewModel = planViewModel;
            _moneyBoxViewModel = moneyBoxViewModel;
        }

        public void Navigate(ViewModelBase currentViewModel, ViewID view)
        {
            switch (view)
            {
                case ViewID.DashboardView:
                    _mainViewModel.CurrentView = _mainViewModel;
                    break;

                case ViewID.AddView:
                    _mainViewModel.CurrentView = _addViewModel;
                    break;
                
                case ViewID.HistoryView:
                    _mainViewModel.CurrentView = _historyViewModel;
                    break;

                case ViewID.PlanView:
                    _mainViewModel.CurrentView = _planViewModel;
                    break;

                case ViewID.MoneyBoxView:
                    _mainViewModel.CurrentView = _moneyBoxViewModel;
                    break;
            }
        }
    }
}
