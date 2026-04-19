using My_money.Enums;

namespace My_money.ViewModel
{
    public class HouseholdMemberListItem : ViewModelBase
    {
        public int MemberId { get; init; }
        public int UserId { get; init; }

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
            set
            {
                SetProperty(ref email, value);
                OnPropertyChanged(nameof(EmailDisplay));
            }
        }

        private string role = nameof(HouseholdMemberRole.Partner);
        public string Role
        {
            get => role;
            set
            {
                if (role == value)
                {
                    return;
                }

                SetProperty(ref role, value);

                if (role == nameof(HouseholdMemberRole.Child))
                {
                    CanManageBudgetEnabled = false;
                    CanManageMembersEnabled = false;
                }
                else if (role == nameof(HouseholdMemberRole.Admin))
                {
                    CanManageBudgetEnabled = true;
                    CanManageMembersEnabled = true;
                }

                OnPropertyChanged(nameof(IsChild));
                OnPropertyChanged(nameof(IsAdult));
            }
        }

        private bool canManageBudgetEnabled;
        public bool CanManageBudgetEnabled
        {
            get => canManageBudgetEnabled;
            set => SetProperty(ref canManageBudgetEnabled, value);
        }

        private bool canManageMembersEnabled;
        public bool CanManageMembersEnabled
        {
            get => canManageMembersEnabled;
            set => SetProperty(ref canManageMembersEnabled, value);
        }

        public string EmailDisplay => Email;
        public bool IsChild => Role == nameof(HouseholdMemberRole.Child);
        public bool IsAdult => !IsChild;
    }
}
