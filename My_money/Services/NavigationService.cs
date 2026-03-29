using System;
using Microsoft.Extensions.DependencyInjection;
using My_money.ViewModel;
using My_money.Views;

namespace My_money.Services
{
    public class NavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Navigate(ViewID view)
        {
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            switch (view)
            {
                case ViewID.DashboardView:
                    mainViewModel.CurrentView = _serviceProvider.GetRequiredService<DashboardViewModel>();
                    break;

                case ViewID.AddView:
                    mainViewModel.CurrentView = _serviceProvider.GetRequiredService<AddViewModel>();
                    break;

                case ViewID.HistoryView:
                    mainViewModel.CurrentView = _serviceProvider.GetRequiredService<HistoryViewModel>();
                    break;

                case ViewID.PlanView:
                    mainViewModel.CurrentView = _serviceProvider.GetRequiredService<PlanViewModel>();
                    break;

                case ViewID.MoneyBoxView:
                    mainViewModel.CurrentView = _serviceProvider.GetRequiredService<MoneyBoxViewModel>();
                    break;
            }
        }
    }
}
