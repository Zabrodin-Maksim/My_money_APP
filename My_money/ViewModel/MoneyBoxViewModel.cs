using My_money.Constants;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class MoneyBoxViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly ISavingsGoalService _savingsGoalService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IHouseholdFinanceService _householdFinanceService;
        private readonly IUserSessionService _userSessionService;
        #endregion

        public MoneyBoxViewModel(
            ISavingsGoalService savingsGoalService,
            IUserFinanceService userFinanceService,
            IHouseholdFinanceService householdFinanceService,
            IUserSessionService userSessionService)
        {
            #region Dependency Injection
            _savingsGoalService = savingsGoalService;
            _userFinanceService = userFinanceService;
            _householdFinanceService = householdFinanceService;
            _userSessionService = userSessionService;
            #endregion

            #region Commands
            AddCommand = new MyICommand<object>(OnAdd);
            DeleteCommand = new MyICommand<object>(OnDelete);
            UpdateCommand = new MyICommand<object>(OnUpdate);
            #endregion

            InitializeContexts();
            _ = LoadDataAsync();
        }

        #region Commands
        public MyICommand<object> AddCommand { get; }
        public MyICommand<object> DeleteCommand { get; }
        public MyICommand<object> UpdateCommand { get; }
        #endregion

        #region Properties
        private ObservableCollection<ContextOption> availableContexts = new();
        public ObservableCollection<ContextOption> AvailableContexts
        {
            get => availableContexts;
            set => SetProperty(ref availableContexts, value);
        }

        private ContextOption selectedContext;
        public ContextOption SelectedContext
        {
            get => selectedContext;
            set
            {
                if (selectedContext == value || value is null)
                {
                    return;
                }

                SetProperty(ref selectedContext, value);
                OnPropertyChanged(nameof(ContextDescription));
                _ = LoadDataAsync();
            }
        }

        private ObservableCollection<SavingsGoal> savingsGoals = new();
        public ObservableCollection<SavingsGoal> SavingsGoals
        {
            get => savingsGoals;
            set => SetProperty(ref savingsGoals, value);
        }

        private SavingsGoal selectedItem;
        public SavingsGoal SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        private decimal totalSavings;
        public decimal TotalSavings
        {
            get => totalSavings;
            set => SetProperty(ref totalSavings, value);
        }

        private decimal notUsedMoney;
        public decimal NotUsedMoney
        {
            get => notUsedMoney;
            set => SetProperty(ref notUsedMoney, value);
        }
        #endregion

        #region Computed Properties
        public string ContextDescription => SelectedContext?.FilterType switch
        {
            CategoryFilterType.Household => "Shared savings goals for the whole household.",
            CategoryFilterType.Personal => "Private savings goals visible only to the signed-in user.",
            CategoryFilterType.Child when IsChild => "Shared goals created by this child account.",
            CategoryFilterType.Child => "Shared goals created by the signed-in user.",
            _ => "Savings goals overview"
        };
        public bool CanManageGoals => !IsChild;
        public bool IsReadOnlyGoals => IsChild;
        public string SavingsCaption => SelectedContext?.UsesHouseholdFinance == true ? "Household savings" : "Personal savings";
        public string AvailableCaption => SelectedContext?.UsesHouseholdFinance == true ? "Available for shared goals" : "Available for private goals";

        private int HouseholdId => _userSessionService.CurrentHouseholdMember?.HouseholdId ?? 0;
        private int UserId => _userSessionService.CurrentUser?.Id ?? 0;
        private bool IsChild => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child);
        #endregion

        private void InitializeContexts()
        {
            AvailableContexts.Clear();
            AvailableContexts.Add(new ContextOption
            {
                Title = "Household",
                FilterType = CategoryFilterType.Household,
                UsesHouseholdFinance = true
            });

            if (!IsChild)
            {
                AvailableContexts.Add(new ContextOption
                {
                    Title = "Personal",
                    FilterType = CategoryFilterType.Personal,
                    UsesHouseholdFinance = false
                });
            }
            else
            {
                AvailableContexts.Add(new ContextOption
                {
                    Title = "My shared activity",
                    FilterType = CategoryFilterType.Child,
                    UsesHouseholdFinance = true
                });
            }

            SelectedContext = AvailableContexts.First();
            OnPropertyChanged(nameof(CanManageGoals));
            OnPropertyChanged(nameof(IsReadOnlyGoals));
        }

        private async Task LoadDataAsync()
        {
            SavingsGoals = SelectedContext.FilterType switch
            {
                CategoryFilterType.Household => new ObservableCollection<SavingsGoal>(await _savingsGoalService.GetAllByHouseholdIdAsync(HouseholdId)),
                CategoryFilterType.Personal => new ObservableCollection<SavingsGoal>(await _savingsGoalService.GetAllByOwnerAsync()),
                CategoryFilterType.Child => new ObservableCollection<SavingsGoal>(await _savingsGoalService.GetAllByHouseholdAndCreatedByAsync(HouseholdId)),
                _ => new ObservableCollection<SavingsGoal>()
            };

            if (SelectedContext.UsesHouseholdFinance)
            {
                var householdFinance = await _householdFinanceService.GetHouseholdFinanceByHouseholdIdAsync(HouseholdId);
                TotalSavings = householdFinance?.Savings ?? 0m;
            }
            else
            {
                var userFinance = await _userFinanceService.GetByUserIdAsync(UserId);
                TotalSavings = userFinance?.Savings ?? 0m;
            }

            NotUsedMoney = TotalSavings - SavingsGoals.Sum(goal => goal.Have);

            OnPropertyChanged(nameof(SavingsCaption));
            OnPropertyChanged(nameof(AvailableCaption));
        }

        #region Commands Implementation
        private async Task OnAdd(object _)
        {
            if (IsChild)
            {
                MessageBox.Show("Child accounts can review savings goals, but they cannot create them.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var goal = new SavingsGoal
                {
                    GoalName = "New goal",
                    Have = 0m,
                    Need = 0m,
                    Scope = SelectedContext.FilterType == CategoryFilterType.Personal ? RecordConstants.Scopes.Personal : RecordConstants.Scopes.Shared,
                    HouseholdId = HouseholdId,
                    OwnerUserId = SelectedContext.FilterType == CategoryFilterType.Personal ? UserId : null,
                    CreatedByUserId = UserId
                };

                await _savingsGoalService.AddSavingsGoal(goal);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Detected in Add Savings Goal", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnDelete(object _)
        {
            if (IsChild)
            {
                MessageBox.Show("Child accounts cannot delete savings goals.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedItem is null)
            {
                MessageBox.Show("Please, select the item.", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _savingsGoalService.DeleteSavingsGoal(SelectedItem.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while deleting the savings goal: " + ex.Message, "Error Detected in Delete Savings Goal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnUpdate(object _)
        {
            if (IsChild)
            {
                MessageBox.Show("Child accounts cannot edit savings goals.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedItem is null)
            {
                MessageBox.Show("Please, select the item.", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _savingsGoalService.UpdateSavingsGoal(SelectedItem);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await LoadDataAsync();
                MessageBox.Show("An error occurred while updating the savings goal: " + ex.Message, "Error Detected in Update Savings Goal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
