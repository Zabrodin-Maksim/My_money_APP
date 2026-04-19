using System;
using System.Threading.Tasks;
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

            mainViewModel.CurrentView = view switch
            {
                ViewID.LoginView => _serviceProvider.GetRequiredService<LoginViewModel>(),
                ViewID.RegistrationView => _serviceProvider.GetRequiredService<RegistrationViewModel>(),
                ViewID.DashboardView => _serviceProvider.GetRequiredService<DashboardViewModel>(),
                ViewID.AddView => _serviceProvider.GetRequiredService<AddViewModel>(),
                ViewID.HistoryView => _serviceProvider.GetRequiredService<HistoryViewModel>(),
                ViewID.PlanView => _serviceProvider.GetRequiredService<PlanViewModel>(),
                ViewID.MoneyBoxView => _serviceProvider.GetRequiredService<MoneyBoxViewModel>(),
                ViewID.HouseholdMembersView => _serviceProvider.GetRequiredService<HouseholdMembersViewModel>(),
                ViewID.SettingsView => _serviceProvider.GetRequiredService<SettingsViewModel>(),
                _ => throw new ArgumentOutOfRangeException(nameof(view), view, null)
            };

            _ = mainViewModel.RefreshSessionContextAsync();
        }

        public Task RefreshShellContextAsync()
        {
            return _serviceProvider.GetRequiredService<MainViewModel>().RefreshSessionContextAsync();
        }
    }
}
