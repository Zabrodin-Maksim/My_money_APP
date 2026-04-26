using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly Services.NavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IHouseholdService _householdService;
        private readonly IFinancialHealthScoreService _financialHealthScoreService;
        #endregion

        public MainViewModel(
            Services.NavigationService navigationService,
            IUserSessionService userSessionService,
            IHouseholdService householdService,
            IFinancialHealthScoreService financialHealthScoreService)
        {
            #region Dependency Injection
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _householdService = householdService;
            _financialHealthScoreService = financialHealthScoreService;
            #endregion

            #region Commands
            NavigateToDashboard = new MyICommand<object>(NavigateToDashboardView);
            NavigateToHistory = new MyICommand<object>(NavigateToHistoryView);
            NavigateToPlan = new MyICommand<object>(NavigateToPlanView);
            NavigateToMoneyBox = new MyICommand<object>(NavigateToMoneyBoxView);
            NavigateToMembers = new MyICommand<object>(NavigateToMembersView);
            NavigateToSettings = new MyICommand<object>(NavigateToSettingsView);
            LogoutCommand = new MyICommand<object>(OnLogout);
            ExitCommand = new MyICommand<object>(OnExit);
            MinimizeWindowCommand = new MyICommand<object>(MinimizeWindow);
            #endregion

            _ = RefreshSessionContextAsync();
        }

        #region Commands
        public MyICommand<object> NavigateToDashboard { get; }
        public MyICommand<object> NavigateToHistory { get; }
        public MyICommand<object> NavigateToPlan { get; }
        public MyICommand<object> NavigateToMoneyBox { get; }
        public MyICommand<object> NavigateToMembers { get; }
        public MyICommand<object> NavigateToSettings { get; }
        public MyICommand<object> LogoutCommand { get; }
        public MyICommand<object> ExitCommand { get; }
        public MyICommand<object> MinimizeWindowCommand { get; }
        #endregion

        #region Properties
        private ViewModelBase currentView;
        public ViewModelBase CurrentView
        {
            get => currentView;
            set => SetProperty(ref currentView, value);
        }

        private string currentUserName = "Guest";
        public string CurrentUserName
        {
            get => currentUserName;
            set => SetProperty(ref currentUserName, value);
        }

        private string currentRole = "Not signed in";
        public string CurrentRole
        {
            get => currentRole;
            set => SetProperty(ref currentRole, value);
        }

        private string currentHouseholdName = "No household";
        public string CurrentHouseholdName
        {
            get => currentHouseholdName;
            set => SetProperty(ref currentHouseholdName, value);
        }

        private string sessionSummary = "Sign in to access your household workspace.";
        public string SessionSummary
        {
            get => sessionSummary;
            set => SetProperty(ref sessionSummary, value);
        }

        private int financialHealthScore;
        public int FinancialHealthScore
        {
            get => financialHealthScore;
            set => SetProperty(ref financialHealthScore, value);
        }
        #endregion

        #region Computed Properties
        public bool IsAuthenticated => _userSessionService.IsAuthenticated;
        public bool IsChild => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child);
        public bool CanManageBudget => !IsChild && _userSessionService.CurrentHouseholdMember?.CanManageBudget == 1;
        public bool CanManageMembers => !IsChild && _userSessionService.CurrentHouseholdMember?.CanManageMembers == 1;
        public bool MustChangePassword => _userSessionService.CurrentUser?.IsActive == 0;
        public bool CanUseFinanceWorkspace => IsAuthenticated && !MustChangePassword;
        public Visibility AuthenticatedVisibility => IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MembersVisibility => CanManageMembers && !MustChangePassword ? Visibility.Visible : Visibility.Collapsed;
        #endregion

        public async Task RefreshSessionContextAsync()
        {
            if (!_userSessionService.IsAuthenticated || _userSessionService.CurrentUser is null)
            {
                CurrentUserName = "Guest";
                CurrentRole = "Not signed in";
                CurrentHouseholdName = "No household";
                FinancialHealthScore = 0;
                SessionSummary = "Sign in or register the first household admin account to begin.";
            }
            else
            {
                CurrentUserName = _userSessionService.CurrentUser.DisplayName;
                CurrentRole = _userSessionService.CurrentHouseholdMember?.Role ?? nameof(HouseholdMemberRole.Partner);

                Household? household = await _householdService.GetHouseholdByAuthenticatedUserAsync();
                CurrentHouseholdName = household?.Name ?? "No household";
                FinancialHealthScore = _userSessionService.CurrentHouseholdMember is null
                    ? 0
                    : await _financialHealthScoreService.GetFinancialHealthScoreAsync(_userSessionService.CurrentHouseholdMember);

                SessionSummary = MustChangePassword
                    ? "Password reset is required before the rest of the workspace becomes available."
                    : CurrentRole switch
                    {
                        nameof(HouseholdMemberRole.Admin) => "Admin access to shared finances, members, and household management.",
                        nameof(HouseholdMemberRole.Partner) => "Partner access to shared finances and private records.",
                        nameof(HouseholdMemberRole.Child) => "Child access focused on shared contributions and read-only planning.",
                        _ => "Authenticated household session."
                    };
            }

            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsChild));
            OnPropertyChanged(nameof(CanManageBudget));
            OnPropertyChanged(nameof(CanManageMembers));
            OnPropertyChanged(nameof(MustChangePassword));
            OnPropertyChanged(nameof(CanUseFinanceWorkspace));
            OnPropertyChanged(nameof(AuthenticatedVisibility));
            OnPropertyChanged(nameof(MembersVisibility));
        }

        #region Navigation Methods
        private Task NavigateToDashboardView(object _)
        {
            _navigationService.Navigate(ViewID.DashboardView);
            return Task.CompletedTask;
        }

        private Task NavigateToHistoryView(object _)
        {
            _navigationService.Navigate(ViewID.HistoryView);
            return Task.CompletedTask;
        }

        private Task NavigateToPlanView(object _)
        {
            _navigationService.Navigate(ViewID.PlanView);
            return Task.CompletedTask;
        }

        private Task NavigateToMoneyBoxView(object _)
        {
            _navigationService.Navigate(ViewID.MoneyBoxView);
            return Task.CompletedTask;
        }

        private Task NavigateToMembersView(object _)
        {
            _navigationService.Navigate(ViewID.HouseholdMembersView);
            return Task.CompletedTask;
        }

        private Task NavigateToSettingsView(object _)
        {
            _navigationService.Navigate(ViewID.SettingsView);
            return Task.CompletedTask;
        }
        #endregion

        #region Command Handlers
        private async Task OnLogout(object _)
        {
            _userSessionService.EndSession();
            await RefreshSessionContextAsync();
            _navigationService.Navigate(ViewID.LoginView);
        }

        private Task OnExit(object _)
        {
            Application.Current.Shutdown();
            return Task.CompletedTask;
        }

        private Task MinimizeWindow(object _)
        {
            SystemCommands.MinimizeWindow(Application.Current.MainWindow);
            return Task.CompletedTask;
        }
        #endregion
    }
}
