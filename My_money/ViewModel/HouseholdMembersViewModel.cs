using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Utilities;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class HouseholdMembersViewModel : ViewModelBase
    {
        private static readonly SKColor[] ScorePalette =
        [
            new(79, 140, 255),
            new(73, 197, 182),
            new(240, 181, 106),
            new(255, 143, 169),
            new(96, 165, 250),
            new(82, 199, 140)
        ];

        #region Dependency Injection Services
        private readonly IHouseholdMemberService _householdMemberService;
        private readonly IUserService _userService;
        private readonly IRegistrationService _registrationService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IUserSessionService _userSessionService;
        private readonly IFinancialHealthScoreService _financialHealthScoreService;
        #endregion

        public HouseholdMembersViewModel(
            IHouseholdMemberService householdMemberService,
            IUserService userService,
            IRegistrationService registrationService,
            IPasswordResetService passwordResetService,
            IUserFinanceService userFinanceService,
            IUserSessionService userSessionService,
            IFinancialHealthScoreService financialHealthScoreService)
        {
            #region Dependency Injection
            _householdMemberService = householdMemberService;
            _userService = userService;
            _registrationService = registrationService;
            _passwordResetService = passwordResetService;
            _userFinanceService = userFinanceService;
            _userSessionService = userSessionService;
            _financialHealthScoreService = financialHealthScoreService;
            #endregion

            #region Commands
            AddMemberCommand = new MyICommand<object>(OnAddMember);
            SaveSelectedMemberCommand = new MyICommand<object>(OnSaveSelectedMember);
            DeleteSelectedMemberCommand = new MyICommand<object>(OnDeleteSelectedMember);
            SendTemporaryPasswordCommand = new MyICommand<object>(OnSendTemporaryPassword);
            UpdateChildPasswordCommand = new MyICommand<object>(OnUpdateChildPassword);
            #endregion

            foreach (var role in Enum.GetValues(typeof(HouseholdMemberRole)).Cast<HouseholdMemberRole>())
            {
                RoleOptions.Add(role);
                MemberRoleNames.Add(role.ToString());
            }

            SelectedRole = HouseholdMemberRole.Partner;
            InitializeCharts();
            _ = LoadMembersAsync();
        }

        #region Commands
        public MyICommand<object> AddMemberCommand { get; }
        public MyICommand<object> SaveSelectedMemberCommand { get; }
        public MyICommand<object> DeleteSelectedMemberCommand { get; }
        public MyICommand<object> SendTemporaryPasswordCommand { get; }
        public MyICommand<object> UpdateChildPasswordCommand { get; }
        #endregion

        #region Properties
        public ObservableCollection<HouseholdMemberListItem> Members { get; } = new();
        public ObservableCollection<HouseholdMemberRole> RoleOptions { get; } = new();
        public ObservableCollection<string> MemberRoleNames { get; } = new();

        private HouseholdMemberListItem selectedMember;
        public HouseholdMemberListItem SelectedMember
        {
            get => selectedMember;
            set
            {
                if (selectedMember is not null)
                {
                    selectedMember.PropertyChanged -= OnSelectedMemberPropertyChanged;
                }

                SetProperty(ref selectedMember, value);

                if (selectedMember is not null)
                {
                    selectedMember.PropertyChanged += OnSelectedMemberPropertyChanged;
                }

                OnPropertyChanged(nameof(SelectedMemberIsChild));
                OnPropertyChanged(nameof(SelectedMemberIsAdult));
                OnPropertyChanged(nameof(CanEditSelectedMember));
            }
        }

        public bool SelectedMemberIsChild => SelectedMember?.Role == nameof(HouseholdMemberRole.Child);
        public bool SelectedMemberIsAdult => SelectedMember is not null && SelectedMember.Role != nameof(HouseholdMemberRole.Child);
        public bool CanEditSelectedMember => IsAdmin && SelectedMember is not null;
        public bool IsAdmin => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Admin);
        public Visibility AdminVisibility => IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        public bool CanEditMembersTable => IsAdmin;

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
                OnPropertyChanged(nameof(NewMemberUsesEmail));

                if (selectedRole == HouseholdMemberRole.Child)
                {
                    NewMemberEmail = string.Empty;
                }
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
        public bool NewMemberUsesEmail => SelectedRole != HouseholdMemberRole.Child;
        public bool CanManageMembers => _userSessionService.CurrentHouseholdMember?.Role != nameof(HouseholdMemberRole.Child)
            && _userSessionService.CurrentHouseholdMember?.CanManageMembers == 1;

        private ISeries[] memberScoreSeries = [];
        public ISeries[] MemberScoreSeries
        {
            get => memberScoreSeries;
            set => SetProperty(ref memberScoreSeries, value);
        }

        private Axis[] memberScoreXAxes = [];
        public Axis[] MemberScoreXAxes
        {
            get => memberScoreXAxes;
            set => SetProperty(ref memberScoreXAxes, value);
        }

        private Axis[] memberScoreYAxes = [];
        public Axis[] MemberScoreYAxes
        {
            get => memberScoreYAxes;
            set => SetProperty(ref memberScoreYAxes, value);
        }

        private string memberScoreInsight = "Scores will appear here once the household member list is loaded.";
        public string MemberScoreInsight
        {
            get => memberScoreInsight;
            set => SetProperty(ref memberScoreInsight, value);
        }
        #endregion

        #region Chart Configuration
        private void InitializeCharts()
        {
            var axisTextPaint = new SolidColorPaint(new SKColor(243, 246, 255));
            var separatorPaint = new SolidColorPaint(new SKColor(120, 132, 165, 90), 1);

            MemberScoreXAxes =
            [
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    MinStep = 20,
                    Labeler = value => value.ToString("0"),
                    LabelsPaint = axisTextPaint,
                    SeparatorsPaint = separatorPaint
                }
            ];

            MemberScoreYAxes =
            [
                new Axis
                {
                    Labels = [],
                    LabelsPaint = axisTextPaint,
                    SeparatorsPaint = null
                }
            ];
        }
        #endregion

        #region Data Loading
        private async Task LoadMembersAsync()
        {
            int? previouslySelectedMemberId = SelectedMember?.MemberId;
            Members.Clear();

            var users = await _userService.GetAllUsersAsync();
            var members = await _householdMemberService.GetAllHouseholdMembersByHouseholdIdAsync();
            var memberItems = await Task.WhenAll(members.Select(async member =>
            {
                var user = users.FirstOrDefault(u => u.Id == member.UserId);
                if (user is null)
                {
                    return null;
                }

                return new HouseholdMemberListItem
                {
                    MemberId = member.Id,
                    UserId = member.UserId,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Role = member.Role,
                    CanManageBudgetEnabled = member.CanManageBudget == 1,
                    CanManageMembersEnabled = member.CanManageMembers == 1,
                    FinancialHealthScore = await _financialHealthScoreService.GetFinancialHealthScoreAsync(member)
                };
            }));

            foreach (var memberItem in memberItems
                .Where(item => item is not null)
                .Cast<HouseholdMemberListItem>()
                .OrderByDescending(item => item.FinancialHealthScore)
                .ThenBy(item => item.DisplayName))
            {
                Members.Add(memberItem);
            }

            UpdateMemberScoreChart();

            if (previouslySelectedMemberId.HasValue)
            {
                SelectedMember = Members.FirstOrDefault(member => member.MemberId == previouslySelectedMemberId.Value);
            }
        }
        #endregion

        #region Chart Updates
        private void UpdateMemberScoreChart()
        {
            var rankedMembers = Members
                .OrderByDescending(member => member.FinancialHealthScore)
                .ThenBy(member => member.DisplayName)
                .ToList();

            if (rankedMembers.Count == 0)
            {
                MemberScoreSeries =
                [
                    new RowSeries<HouseholdMemberScorePoint>
                    {
                        Name = "Score",
                        Values =
                        [
                            new HouseholdMemberScorePoint("No members", 0, new SolidColorPaint(ScorePalette[0]))
                        ],
                        DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                        DataLabelsPosition = DataLabelsPosition.End,
                        DataLabelsTranslate = new(-1, 0),
                        DataLabelsFormatter = point => "0",
                        MaxBarWidth = 36,
                        Padding = 8
                    }
                    .OnPointMeasured(point =>
                    {
                        if (point.Visual is null)
                        {
                            return;
                        }

                        point.Visual.Fill = ((HouseholdMemberScorePoint)point.Model!).Paint;
                    })
                ];

                MemberScoreYAxes =
                [
                    new Axis
                    {
                        Labels = ["No members"],
                        LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                        SeparatorsPaint = null
                    }
                ];

                MemberScoreInsight = "There are no household members to compare yet.";
                return;
            }

            var scorePoints = rankedMembers
                .Select((member, index) => new HouseholdMemberScorePoint(
                    member.DisplayName,
                    member.FinancialHealthScore,
                    new SolidColorPaint(ScorePalette[index % ScorePalette.Length])))
                .Reverse()
                .ToArray();

            MemberScoreSeries =
            [
                new RowSeries<HouseholdMemberScorePoint>
                {
                    Name = "Financial health score",
                    Values = [.. scorePoints],
                    DataLabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    DataLabelsPosition = DataLabelsPosition.End,
                    DataLabelsFormatter = point => $"{((HouseholdMemberScorePoint)point.Model!).Value:0}",
                    MaxBarWidth = 36,
                    Padding = 8,
                }
                .OnPointMeasured(point =>
                {
                    if (point.Visual is null)
                    {
                        return;
                    }

                    point.Visual.Fill = ((HouseholdMemberScorePoint)point.Model!).Paint;
                })
            ];

            MemberScoreYAxes =
            [
                new Axis
                {
                    Labels = [.. scorePoints.Select(point => point.Name)],
                    LabelsPaint = new SolidColorPaint(new SKColor(243, 246, 255)),
                    SeparatorsPaint = null
                }
            ];

            var leader = rankedMembers.First();
            var challenger = rankedMembers.Skip(1).FirstOrDefault();

            MemberScoreInsight = challenger is null
                ? $"{leader.DisplayName} currently sets the household benchmark with a score of {leader.FinancialHealthScore}."
                : $"{leader.DisplayName} leads the household with {leader.FinancialHealthScore}, staying {leader.FinancialHealthScore - challenger.FinancialHealthScore} points ahead of {challenger.DisplayName}.";
        }
        #endregion

        #region Chart Models
        private sealed class HouseholdMemberScorePoint : ObservableValue
        {
            public HouseholdMemberScorePoint(string name, double value, SolidColorPaint paint)
            {
                Name = name;
                Paint = paint;
                Value = value;
            }

            public string Name { get; }
            public SolidColorPaint Paint { get; }
        }
        #endregion

        #region Property Changed Handlers
        private void OnSelectedMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(HouseholdMemberListItem.Role) or nameof(HouseholdMemberListItem.CanManageBudgetEnabled) or nameof(HouseholdMemberListItem.CanManageMembersEnabled))
            {
                OnPropertyChanged(nameof(SelectedMemberIsChild));
                OnPropertyChanged(nameof(SelectedMemberIsAdult));
            }
        }
        #endregion

        #region Command Handlers
        private async Task OnAddMember(object _)
        {
            try
            {
                if (!IsAdmin)
                {
                    throw new InvalidOperationException("Only admins can add household members.");
                }

                if (string.IsNullOrWhiteSpace(NewMemberName))
                {
                    throw new InvalidOperationException("Name is required.");
                }

                string? passwordHash = null;
                var email = NewMemberEmail.Trim();

                if (SelectedRole == HouseholdMemberRole.Child)
                {
                    if (string.IsNullOrWhiteSpace(ChildInitialPassword))
                    {
                        throw new InvalidOperationException("Admin must set an initial password for child accounts.");
                    }

                    passwordHash = PasswordHasher.HashPassword(ChildInitialPassword);
                    email = NewMemberName.Trim();

                    if (await _userService.GetUserByEmailAsync(email) is not null)
                    {
                        throw new InvalidOperationException("A child login with this name already exists. Choose a different name.");
                    }
                }
                else if (string.IsNullOrWhiteSpace(email))
                {
                    throw new InvalidOperationException("Email is required for adult accounts.");
                }

                await _registrationService.RegisterUserAsync(passwordHash, NewMemberName.Trim(), email, SelectedRole);

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

        private async Task OnSaveSelectedMember(object _)
        {
            try
            {
                if (!IsAdmin)
                {
                    throw new InvalidOperationException("Only admins can edit member roles, rights, and names.");
                }

                if (SelectedMember is null)
                {
                    throw new InvalidOperationException("Select a member first.");
                }

                if (string.IsNullOrWhiteSpace(SelectedMember.DisplayName))
                {
                    throw new InvalidOperationException("Display name cannot be empty.");
                }

                var user = await _userService.GetUserByIdAsync(SelectedMember.UserId)
                    ?? throw new InvalidOperationException("The selected user was not found.");
                var member = await _householdMemberService.GetHouseholdMemberByIdAsync(SelectedMember.MemberId)
                    ?? throw new InvalidOperationException("The selected household membership was not found.");

                user.DisplayName = SelectedMember.DisplayName.Trim();
                member.Role = SelectedMember.Role;
                member.CanManageBudget = SelectedMember.CanManageBudgetEnabled ? 1 : 0;
                member.CanManageMembers = SelectedMember.CanManageMembersEnabled ? 1 : 0;

                if (member.Role == nameof(HouseholdMemberRole.Child))
                {
                    member.CanManageBudget = 0;
                    member.CanManageMembers = 0;
                }
                else if (member.Role == nameof(HouseholdMemberRole.Admin))
                {
                    member.CanManageBudget = 1;
                    member.CanManageMembers = 1;
                }

                await _userService.UpdateUserAsync(user);
                await _householdMemberService.UpdateHouseholdMemberAsync(member);
                await LoadMembersAsync();

                SelectedMember = Members.FirstOrDefault(x => x.MemberId == member.Id);
                MessageBox.Show("Member details updated successfully.", "Member updated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to update member", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnSendTemporaryPassword(object _)
        {
            try
            {
                if (!IsAdmin)
                {
                    throw new InvalidOperationException("Only admins can reset passwords for other members.");
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

        private async Task OnDeleteSelectedMember(object _)
        {
            try
            {
                if (!IsAdmin)
                {
                    throw new InvalidOperationException("Only admins can delete household members.");
                }

                if (SelectedMember is null)
                {
                    throw new InvalidOperationException("Select a member first.");
                }

                if (SelectedMember.UserId == _userSessionService.CurrentUser?.Id)
                {
                    throw new InvalidOperationException("Admin cannot delete their own household membership.");
                }

                var result = MessageBox.Show(
                    $"Delete member '{SelectedMember.DisplayName}' from the household?",
                    "Delete member",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                var userFinance = await _userFinanceService.GetByUserIdAsync(SelectedMember.UserId);
                if (userFinance is not null)
                {
                    await _userFinanceService.DeleteUserFinance(userFinance.Id);
                }

                await _householdMemberService.DeleteHouseholdMemberAsync(SelectedMember.MemberId);
                await _userService.DeleteUserAsync(SelectedMember.UserId);

                SelectedMember = null;
                await LoadMembersAsync();
                MessageBox.Show("Member deleted successfully.", "Member deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to delete member", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnUpdateChildPassword(object _)
        {
            try
            {
                if (!IsAdmin)
                {
                    throw new InvalidOperationException("Only admins can change child passwords.");
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
        #endregion
    }
}
