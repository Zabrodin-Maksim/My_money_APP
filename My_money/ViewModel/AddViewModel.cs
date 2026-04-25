using My_money.Constants;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IRecordService _recordService;
        private readonly IUserSessionService _userSessionService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public AddViewModel(
            IBudgetCategoryService budgetCategoryService,
            IRecordService recordService,
            IUserSessionService userSessionService,
            Services.NavigationService navigationService)
        {
            #region Dependency Injection
            _budgetCategoryService = budgetCategoryService;
            _recordService = recordService;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            #endregion

            #region Commands
            AddCommand = new MyICommand<object>(OnAdd);
            BackCommand = new MyICommand<string>(OnBack);
            #endregion

            InitializeOptions();
            _ = InitDataAsync();
        }

        #region Commands
        public MyICommand<object> AddCommand { get; }
        public MyICommand<string> BackCommand { get; }
        #endregion

        #region Properties
        public ObservableCollection<string> ScopeOptions { get; } = new();
        public ObservableCollection<string> RecordTypeOptions { get; } = new();
        public ObservableCollection<string> IncomeTargetOptions { get; } = new()
        {
            nameof(IncomeTarget.Balance),
            nameof(IncomeTarget.Savings)
        };

        private ObservableCollection<BudgetCategory> categories = new();
        public ObservableCollection<BudgetCategory> Categories
        {
            get => categories;
            set => SetProperty(ref categories, value);
        }

        private BudgetCategory selectedCategory;
        public BudgetCategory SelectedCategory
        {
            get => selectedCategory;
            set => SetProperty(ref selectedCategory, value);
        }

        private string selectedScope;
        public string SelectedScope
        {
            get => selectedScope;
            set
            {
                if (selectedScope == value || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                SetProperty(ref selectedScope, value);
                _ = LoadCategoriesAsync();
            }
        }

        private string selectedRecordType;
        public string SelectedRecordType
        {
            get => selectedRecordType;
            set
            {
                if (selectedRecordType == value || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                SetProperty(ref selectedRecordType, value);
                OnPropertyChanged(nameof(IncomeTargetVisibility));
                OnPropertyChanged(nameof(ExpenseTargetVisibility));
            }
        }

        private string selectedIncomeTarget = nameof(IncomeTarget.Balance);
        public string SelectedIncomeTarget
        {
            get => selectedIncomeTarget;
            set
            {
                SetProperty(ref selectedIncomeTarget, value);
            }
        }

        private string amountTextProperty = "0";
        public string AmountTextProperty
        {
            get => amountTextProperty;
            set
            {
                SetProperty(ref amountTextProperty, value);
            }
        }

        private string descriptionProperty = string.Empty;
        public string DescriptionProperty
        {
            get => descriptionProperty;
            set => SetProperty(ref descriptionProperty, value);
        }

        private DateTime? selectedDate = DateTime.Now;
        public DateTime? SelectedDate
        {
            get => selectedDate;
            set => SetProperty(ref selectedDate, value);
        }

        public Visibility IncomeTargetVisibility => SelectedRecordType == RecordConstants.Types.Income
            ? Visibility.Visible
            : Visibility.Collapsed;
        public Visibility ExpenseTargetVisibility => SelectedRecordType == RecordConstants.Types.Expense
            ? Visibility.Visible
            : Visibility.Collapsed;
        #endregion

        private int HouseholdId => _userSessionService.CurrentHouseholdMember?.HouseholdId ?? 0;
        private int UserId => _userSessionService.CurrentUser?.Id ?? 0;
        private bool IsChild => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child);

        private void InitializeOptions()
        {
            ScopeOptions.Clear();
            ScopeOptions.Add(RecordConstants.Scopes.Shared);

            if (!IsChild)
            {
                ScopeOptions.Add(RecordConstants.Scopes.Personal);
            }

            RecordTypeOptions.Clear();
            RecordTypeOptions.Add(RecordConstants.Types.Expense);
            RecordTypeOptions.Add(RecordConstants.Types.Income);

            SelectedScope = ScopeOptions.First();
            SelectedRecordType = RecordTypeOptions.First();
            SelectedIncomeTarget = IncomeTargetOptions.First();
        }

        private async Task InitDataAsync()
        {
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            Categories = SelectedScope == RecordConstants.Scopes.Personal
                ? new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllByOwnerAsync())
                : new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllByHouseholdIdAsync(HouseholdId));

            SelectedCategory = Categories.FirstOrDefault();
        }

        private async Task OnAdd(object _)
        {
            try
            {
                if (!decimal.TryParse(AmountTextProperty, out var amount) || amount <= 0)
                {
                    throw new FormatException("Please enter a valid positive amount.");
                }

                if (!SelectedDate.HasValue)
                {
                    throw new InvalidOperationException("Please select the date of the record.");
                }

                if (SelectedCategory is null)
                {
                    throw new InvalidOperationException("Please select a category.");
                }

                if (IsChild && SelectedScope == RecordConstants.Scopes.Personal)
                {
                    throw new InvalidOperationException("Child accounts cannot create personal records.");
                }

                var record = new Record
                {
                    Amount = amount,
                    CategoryId = SelectedCategory.Id,
                    DateTimeOccurred = SelectedDate,
                    Description = DescriptionProperty,
                    HouseholdId = HouseholdId,
                    OwnerUserId = SelectedScope == RecordConstants.Scopes.Personal ? UserId : null,
                    CreatedByUserId = UserId,
                    Scope = SelectedScope,
                    Type = SelectedRecordType,
                    IncomeTarget = SelectedRecordType == RecordConstants.Types.Income ? SelectedIncomeTarget : null
                };

                await _recordService.AddRecordAsync(record, SelectedCategory);
                Clear();
                await LoadCategoriesAsync();

                MessageBox.Show("Record added successfully.", "Add Record", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning: Error Detected in Add entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Task OnBack(string _)
        {
            _navigationService.Navigate(ViewID.DashboardView);
            return Task.CompletedTask;
        }

        private void Clear()
        {
            AmountTextProperty = "0";
            DescriptionProperty = string.Empty;
            SelectedDate = DateTime.Now;
            SelectedRecordType = RecordTypeOptions.First();
            SelectedIncomeTarget = IncomeTargetOptions.First();
        }
    }
}
