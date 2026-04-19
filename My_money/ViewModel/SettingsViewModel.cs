using My_money.Services.IServices;
using My_money.Utilities;
using My_money.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IUserSessionService _userSessionService;
        private readonly IUserService _userService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public SettingsViewModel(
            IUserSessionService userSessionService,
            IUserService userService,
            IPasswordResetService passwordResetService,
            Services.NavigationService navigationService)
        {
            #region Dependency Injection
            _userSessionService = userSessionService;
            _userService = userService;
            _passwordResetService = passwordResetService;
            _navigationService = navigationService;
            #endregion

            #region Commands
            SaveProfileCommand = new MyICommand<object>(OnSaveProfile);
            ChangePasswordCommand = new MyICommand<object>(OnChangePassword);
            #endregion

            LoadCurrentState();
        }
        
        #region Commands
        public MyICommand<object> SaveProfileCommand { get; }
        public MyICommand<object> ChangePasswordCommand { get; }
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

        private string currentPassword = string.Empty;
        public string CurrentPassword
        {
            get => currentPassword;
            set => SetProperty(ref currentPassword, value);
        }

        private string newPassword = string.Empty;
        public string NewPassword
        {
            get => newPassword;
            set => SetProperty(ref newPassword, value);
        }

        private string confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => confirmPassword;
            set => SetProperty(ref confirmPassword, value);
        }
        #endregion

        #region Computed Properties
        public bool MustChangePassword => _userSessionService.CurrentUser?.IsActive == 0;
        public bool CanChangeOwnPassword => _userSessionService.CurrentHouseholdMember?.Role != nameof(Enums.HouseholdMemberRole.Child);
        public string PasswordHint => MustChangePassword
            ? "This account is using a temporary password. Set a new password to unlock the rest of the workspace."
            : "Use this section to change your own password.";
        #endregion

        private void LoadCurrentState()
        {
            DisplayName = _userSessionService.CurrentUser?.DisplayName ?? string.Empty;
            Email = _userSessionService.CurrentUser?.Email ?? string.Empty;
            OnPropertyChanged(nameof(MustChangePassword));
            OnPropertyChanged(nameof(CanChangeOwnPassword));
            OnPropertyChanged(nameof(PasswordHint));
        }

        #region Command Implementations
        private async Task OnSaveProfile(object _)
        {
            try
            {
                var user = _userSessionService.CurrentUser ?? throw new InvalidOperationException("User session is not available.");
                user.DisplayName = DisplayName.Trim();
                await _userService.UpdateUserAsync(user);
                await _navigationService.RefreshShellContextAsync();
                MessageBox.Show("Display name updated successfully.", "Profile updated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Profile update failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnChangePassword(object _)
        {
            try
            {
                bool wasForcedReset = MustChangePassword;

                if (!CanChangeOwnPassword)
                {
                    throw new InvalidOperationException("Child accounts cannot change their own password.");
                }

                if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    throw new InvalidOperationException("Please fill in the new password and confirmation.");
                }

                if (NewPassword != ConfirmPassword)
                {
                    throw new InvalidOperationException("The password confirmation does not match.");
                }

                var user = _userSessionService.CurrentUser ?? throw new InvalidOperationException("User session is not available.");

                if (!MustChangePassword && !PasswordHasher.VerifyPassword(CurrentPassword, user.PasswordHash))
                {
                    throw new InvalidOperationException("Current password is incorrect.");
                }

                await _passwordResetService.ChangePassword(user, NewPassword);
                await _navigationService.RefreshShellContextAsync();

                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                OnPropertyChanged(nameof(MustChangePassword));
                OnPropertyChanged(nameof(PasswordHint));

                MessageBox.Show("Password updated successfully.", "Password changed", MessageBoxButton.OK, MessageBoxImage.Information);

                if (!wasForcedReset)
                {
                    return;
                }

                _navigationService.Navigate(ViewID.DashboardView);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Password change failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion
    }
}
