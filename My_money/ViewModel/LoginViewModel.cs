using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IAuthService _authService;
        private readonly IHouseholdMemberService _householdMemberService;
        private readonly IUserSessionService _userSessionService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public LoginViewModel(
            IAuthService authService,
            IHouseholdMemberService householdMemberService,
            IUserSessionService userSessionService,
            Services.NavigationService navigationService)
        {
            #region Dependency Injection
            _authService = authService;
            _householdMemberService = householdMemberService;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            #endregion

            #region Commands
            LoginCommand = new MyICommand<object>(OnLogin);
            OpenRegistrationCommand = new MyICommand<object>(OpenRegistration);
            #endregion
        }

        #region Commands
        public MyICommand<object> LoginCommand { get; }
        public MyICommand<object> OpenRegistrationCommand { get; }
        #endregion

        #region Properties
        private string email = string.Empty;
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string statusMessage = "Sign in with your household account.";
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }
        #endregion

        private async Task OnLogin(object _)
        {
            try
            {
                var user = await _authService.AuthUserAsync(Email.Trim(), Password);
                var householdMember = await _householdMemberService.GetHouseholdMemberByUserIdAsync(user.Id)
                    ?? throw new InvalidOperationException("The authenticated user is not assigned to a household.");

                _userSessionService.StartSession(user, householdMember);
                await _navigationService.RefreshShellContextAsync();

                if (user.IsActive == 0)
                {
                    StatusMessage = "Temporary password accepted. Please set a new password before using the workspace.";
                    _navigationService.Navigate(ViewID.SettingsView);
                }
                else
                {
                    _navigationService.Navigate(ViewID.DashboardView);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sign-in failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Task OpenRegistration(object _)
        {
            _navigationService.Navigate(ViewID.RegistrationView);
            return Task.CompletedTask;
        }
    }
}
