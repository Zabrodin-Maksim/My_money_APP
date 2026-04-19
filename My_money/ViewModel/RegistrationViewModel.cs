using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class RegistrationViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IRegistrationService _registrationService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public RegistrationViewModel(IRegistrationService registrationService, Services.NavigationService navigationService)
        {
            #region Dependency Injection
            _registrationService = registrationService;
            _navigationService = navigationService;
            #endregion

            #region Commands
            RegisterCommand = new MyICommand<object>(OnRegister);
            BackToLoginCommand = new MyICommand<object>(BackToLogin);
            #endregion
        }

        #region Commands
        public MyICommand<object> RegisterCommand { get; }
        public MyICommand<object> BackToLoginCommand { get; }
        #endregion

        #region Properties
        private string displayName = string.Empty;
        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }

        private string email = string.Empty;
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string householdName = string.Empty;
        public string HouseholdName
        {
            get => householdName;
            set => SetProperty(ref householdName, value);
        }
        #endregion

        #region Commands Implementation
        private async Task OnRegister(object _)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(HouseholdName))
                {
                    throw new InvalidOperationException("Display name, email, and household name are required.");
                }

                await _registrationService.RegisterAdminAndHouseholdAsync(
                    DisplayName.Trim(),
                    Email.Trim(),
                    new Household
                    {
                        Name = HouseholdName.Trim(),
                        CreatedByUserId = 0
                    });

                MessageBox.Show("The household admin account was created. A temporary password has been sent to the provided email.", "Registration completed", MessageBoxButton.OK, MessageBoxImage.Information);
                _navigationService.Navigate(ViewID.LoginView);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Registration failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Task BackToLogin(object _)
        {
            _navigationService.Navigate(ViewID.LoginView);
            return Task.CompletedTask;
        }
        #endregion
    }
}
