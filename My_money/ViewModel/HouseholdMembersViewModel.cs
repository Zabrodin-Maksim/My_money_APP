using My_money.Enums;
using My_money.Services.IServices;
using My_money.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class HouseholdMembersViewModel : ViewModelBase
    {
        private readonly IHouseholdMemberService _householdMemberService;
        private readonly IUserService _userService;
        private readonly IRegistrationService _registrationService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IUserSessionService _userSessionService;

        public HouseholdMembersViewModel(
            IHouseholdMemberService householdMemberService,
            IUserService userService,
            IRegistrationService registrationService,
            IPasswordResetService passwordResetService,
            IUserSessionService userSessionService)
        {
            _householdMemberService = householdMemberService;
            _userService = userService;
            _registrationService = registrationService;
            _passwordResetService = passwordResetService;
            _userSessionService = userSessionService;

            AddMemberCommand = new MyICommand<object>(OnAddMember);
            SendTemporaryPasswordCommand = new MyICommand<object>(OnSendTemporaryPassword);
            UpdateChildPasswordCommand = new MyICommand<object>(OnUpdateChildPassword);

            foreach (var role in Enum.GetValues(typeof(HouseholdMemberRole)).Cast<HouseholdMemberRole>())
            {
                RoleOptions.Add(role);
            }

            SelectedRole = HouseholdMemberRole.Partner;
            _ = LoadMembersAsync();
        }

        public MyICommand<object> AddMemberCommand { get; }
        public MyICommand<object> SendTemporaryPasswordCommand { get; }
        public MyICommand<object> UpdateChildPasswordCommand { get; }

        public ObservableCollection<HouseholdMemberListItem> Members { get; } = new();
        public ObservableCollection<HouseholdMemberRole> RoleOptions { get; } = new();

        private HouseholdMemberListItem selectedMember;
        public HouseholdMemberListItem SelectedMember
        {
            get => selectedMember;
            set
            {
                SetProperty(ref selectedMember, value);
                OnPropertyChanged(nameof(SelectedMemberIsChild));
                OnPropertyChanged(nameof(SelectedMemberIsAdult));
            }
        }

        public bool SelectedMemberIsChild => SelectedMember?.Role == nameof(HouseholdMemberRole.Child);
        public bool SelectedMemberIsAdult => SelectedMember is not null && SelectedMember.Role != nameof(HouseholdMemberRole.Child);

        private string newMemberName = string.Empty;
        public string NewMemberName
        {
            get => newMemberName;
            set => SetProperty(ref newMemberName, value);
        }

        private string newMemberEmail = string.Empty;
        public string NewMemberEmail
        {
            get => newMemberEmail;
            set => SetProperty(ref newMemberEmail, value);
        }

        private HouseholdMemberRole selectedRole;
        public HouseholdMemberRole SelectedRole
        {
            get => selectedRole;
            set
            {
                SetProperty(ref selectedRole, value);
                OnPropertyChanged(nameof(NewMemberNeedsPassword));
            }
        }

        private string childInitialPassword = string.Empty;
        public string ChildInitialPassword
        {
            get => childInitialPassword;
            set => SetProperty(ref childInitialPassword, value);
        }

        private string newChildPassword = string.Empty;
        public string NewChildPassword
        {
            get => newChildPassword;
            set => SetProperty(ref newChildPassword, value);
        }

        public bool NewMemberNeedsPassword => SelectedRole == HouseholdMemberRole.Child;
        public bool CanManageMembers => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Admin);

        private async Task LoadMembersAsync()
        {
            Members.Clear();

            var users = await _userService.GetAllUsersAsync();
            var members = await _householdMemberService.GetAllHouseholdMembersByHouseholdIdAsync();

            foreach (var member in members)
            {
                var user = users.FirstOrDefault(u => u.Id == member.UserId);
                if (user is null)
                {
                    continue;
                }

                Members.Add(new HouseholdMemberListItem
                {
                    MemberId = member.Id,
                    UserId = member.UserId,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Role = member.Role,
                    CanManageBudget = member.CanManageBudget,
                    CanManageMembers = member.CanManageMembers
                });
            }
        }

        private async Task OnAddMember(object _)
        {
            try
            {
                if (!CanManageMembers)
                {
                    throw new InvalidOperationException("Only admins can manage household members.");
                }

                if (string.IsNullOrWhiteSpace(NewMemberName) || string.IsNullOrWhiteSpace(NewMemberEmail))
                {
                    throw new InvalidOperationException("Name and email are required.");
                }

                string? passwordHash = null;
                if (SelectedRole == HouseholdMemberRole.Child)
                {
                    if (string.IsNullOrWhiteSpace(ChildInitialPassword))
                    {
                        throw new InvalidOperationException("Admin must set an initial password for child accounts.");
                    }

                    passwordHash = PasswordHasher.HashPassword(ChildInitialPassword);
                }

                await _registrationService.RegisterUserAsync(passwordHash, NewMemberName.Trim(), NewMemberEmail.Trim(), SelectedRole);

                NewMemberName = string.Empty;
                NewMemberEmail = string.Empty;
                ChildInitialPassword = string.Empty;

                await LoadMembersAsync();
                MessageBox.Show("Household member created successfully.", "Member created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to add member", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnSendTemporaryPassword(object _)
        {
            try
            {
                if (!CanManageMembers)
                {
                    throw new InvalidOperationException("Only admins can manage household members.");
                }

                if (SelectedMember is null || !SelectedMemberIsAdult)
                {
                    throw new InvalidOperationException("Select an adult member first.");
                }

                await _passwordResetService.ResetPasswordWithTemporary(SelectedMember.Email);
                MessageBox.Show("Temporary password email sent.", "Password reset", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Password reset failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnUpdateChildPassword(object _)
        {
            try
            {
                if (!CanManageMembers)
                {
                    throw new InvalidOperationException("Only admins can manage household members.");
                }

                if (SelectedMember is null || !SelectedMemberIsChild)
                {
                    throw new InvalidOperationException("Select a child member first.");
                }

                if (string.IsNullOrWhiteSpace(NewChildPassword))
                {
                    throw new InvalidOperationException("Enter a new password for the selected child account.");
                }

                var user = await _userService.GetUserByIdAsync(SelectedMember.UserId)
                    ?? throw new InvalidOperationException("The selected child account was not found.");

                await _passwordResetService.ChangePassword(user, NewChildPassword);
                NewChildPassword = string.Empty;
                MessageBox.Show("Child password updated successfully.", "Password changed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Child password update failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
