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
    public class PlanViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserSessionService _userSessionService;
        #endregion

        public PlanViewModel(IBudgetCategoryService budgetCategoryService, IUserSessionService userSessionService)
        {
            #region Dependency Injection
            _budgetCategoryService = budgetCategoryService;
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
                OnPropertyChanged(nameof(CanManageCategories));
                OnPropertyChanged(nameof(IsReadOnlyCategories));
                OnPropertyChanged(nameof(AddCategoryVisibility));
                OnPropertyChanged(nameof(DeleteCategoryVisibility));
                _ = LoadDataAsync();
            }
        }

        private ObservableCollection<BudgetCategory> budgetCategories = new();
        public ObservableCollection<BudgetCategory> BudgetCategories
        {
            get => budgetCategories;
            set => SetProperty(ref budgetCategories, value);
        }

        private BudgetCategory selectedItem;
        public BudgetCategory SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        private decimal totalPlannedBudget;
        public decimal TotalPlannedBudget
        {
            get => totalPlannedBudget;
            set => SetProperty(ref totalPlannedBudget, value);
        }
        #endregion

        #region Computed Properties
        public string ContextDescription => SelectedContext?.FilterType switch
        {
            CategoryFilterType.Household => "Shared household categories and budgets.",
            CategoryFilterType.Personal => "Private categories used only for the signed-in user's finances.",
            CategoryFilterType.Child when IsChild => "Shared categories created by this child account.",
            CategoryFilterType.Child => "Shared categories created by the signed-in user.",
            _ => "Budget plan overview"
        };
        public bool CanManageBudget => !IsChild && _userSessionService.CurrentHouseholdMember?.CanManageBudget == 1;
        public bool CanManageCategories => CanManageBudget && SelectedContext?.FilterType != CategoryFilterType.Child;
        public bool IsReadOnlyCategories => !CanManageCategories;
        public Visibility AddCategoryVisibility => CanManageCategories ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteCategoryVisibility => CanManageCategories ? Visibility.Visible : Visibility.Collapsed;

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
            OnPropertyChanged(nameof(CanManageBudget));
            OnPropertyChanged(nameof(CanManageCategories));
            OnPropertyChanged(nameof(IsReadOnlyCategories));
            OnPropertyChanged(nameof(AddCategoryVisibility));
            OnPropertyChanged(nameof(DeleteCategoryVisibility));
        }

        private async Task LoadDataAsync()
        {
            BudgetCategories = SelectedContext.FilterType switch
            {
                CategoryFilterType.Household => new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllByHouseholdIdAsync(HouseholdId)),
                CategoryFilterType.Personal => new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllByOwnerAsync()),
                CategoryFilterType.Child => new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllByHouseholdAndCreatedByAsync(HouseholdId)),
                _ => new ObservableCollection<BudgetCategory>()
            };

            TotalPlannedBudget = BudgetCategories.Sum(category => category.Plan);
        }

        #region Command Implementations
        private async Task OnDelete(object _)
        {
            if (!CanManageCategories)
            {
                MessageBox.Show("You do not have permission to manage budget categories in this context.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedItem is null)
            {
                MessageBox.Show("Please, select the item.", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _budgetCategoryService.DeleteBudgetCategoryAsync(SelectedItem);
                MessageBox.Show("All records of type " + SelectedItem.Name + " have been moved under the 'Other' type.", "Information: Successful deletion", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Detected on Delete action", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnAdd(object _)
        {
            if (!CanManageCategories)
            {
                MessageBox.Show("You do not have permission to manage budget categories in this context.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var category = new BudgetCategory
                {
                    Name = "New category",
                    Plan = 0m,
                    HouseholdId = HouseholdId,
                    OwnerUserId = SelectedContext.FilterType == CategoryFilterType.Personal ? UserId : null,
                    CreatedByUserId = UserId,
                    Scope = SelectedContext.FilterType == CategoryFilterType.Personal ? RecordConstants.Scopes.Personal : RecordConstants.Scopes.Shared
                };

                await _budgetCategoryService.AddBudgetCategoryAsync(category);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Detected on Add action", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnUpdate(object _)
        {
            if (!CanManageCategories)
            {
                MessageBox.Show("You do not have permission to manage budget categories in this context.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedItem is null)
            {
                MessageBox.Show("Please, select the item.", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _budgetCategoryService.UpdateBudgetCategoryAsync(SelectedItem);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await LoadDataAsync();
                MessageBox.Show("An error occurred while updating the category: " + ex.Message, "Error Detected in Update Category", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion
    }
}
