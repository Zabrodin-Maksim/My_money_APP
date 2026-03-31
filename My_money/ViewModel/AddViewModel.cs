using My_money.Model;
using My_money.Services.IServices;
using My_money.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        // TODO: Add Discription TextBox and indicator for secsessful add

        #region Visivility & Check Boxes
        // Visibility for Type and Date/
        private Visibility visibilityMainInform;
        public Visibility VisibilityMainInform { get { return visibilityMainInform; } set { visibilityMainInform = value; } }

        // Visibility for In Savings
        private Visibility visibilitySavings;
        public Visibility VisibilitySavings { get { return visibilitySavings; } set { visibilitySavings = value; } }

        // Visibility for In Balance
        private Visibility visibilityBalance;
        public Visibility VisibilityBalance { get { return visibilityBalance; } set { visibilityBalance = value; } }


        //Check Boxes
        private bool isSavingsChecked;
        public bool IsSavingsChecked
        {
            get { return isSavingsChecked; }
            set
            {
                SetProperty(ref isSavingsChecked, value);
                UpdateVisibility();
            }
        }

        private bool isBalanceChecked;
        public bool IsBalanceChecked
        {
            get { return isBalanceChecked; }
            set
            {
                SetProperty(ref isBalanceChecked, value);
                UpdateVisibility();
            }
        }
        #endregion

        #region Properties
        private string amountTextProperty;
        public string AmountTextProperty
        {
            get { return amountTextProperty; }
            set { CheckNumericInput(value); }
        }

        private string descriptionProperty;
        public string DescriptionProperty
        {
            get { return descriptionProperty; }
            set { SetProperty(ref descriptionProperty, value); }
        }

        private DateTime? selectedDate = DateTime.Now;
        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set { selectedDate = value; }
        }

        private ObservableCollection<BudgetCategory> categories;
        public ObservableCollection<BudgetCategory> Categories
        {
            get { return categories; }

            set
            {
                SetProperty(ref categories, value);
            }
        }

        private BudgetCategory selectedCategory;
        public BudgetCategory SelectedCategory
        {
            get { return selectedCategory; }
            set { selectedCategory = value; }
        }
        #endregion

        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IUserFinanceService _userFinanceService;
        private readonly IRecordService _recordService;
        private readonly Services.NavigationService _navigationService;
        #endregion

        public AddViewModel(IBudgetCategoryService budgetCategoryService, IUserFinanceService userFinanceService, IRecordService recordService, Services.NavigationService navigationService)
        {
            #region DI Services
            _budgetCategoryService = budgetCategoryService;
            _userFinanceService = userFinanceService;
            _recordService = recordService;
            _navigationService = navigationService;
            #endregion

            AddCommand = new MyICommand<object>(OnAdd);
            BackCommand = new MyICommand<string>(OnBack);

            _ = InitData();
        }

        private async Task InitData()
        {
            AmountTextProperty = "0";
            DescriptionProperty = string.Empty;
            Categories = new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllBudgetCategoriesAsync());
            if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }

        #region Commands
        public MyICommand<object> AddCommand { get; private set; }
        public MyICommand<string> BackCommand { get; private set; }
        #endregion

        #region ADD
        private async Task OnAdd(object parametr)
        {
            try
            {
                if (!decimal.TryParse(amountTextProperty, out var amount))
                {
                    throw new FormatException();
                }

                if (!CheckAddInput(amountTextProperty))
                {
                    return;
                }

                // Add in Savings
                if (isSavingsChecked)
                {
                    await _userFinanceService.AddToSavingsAsync(amount);
                }
                // Add in Balance
                else if (isBalanceChecked)
                {
                    await _userFinanceService.AddToBalanceAsync(amount);
                }
                // Add in Records
                else
                {
                    if (amount < 0)
                    {
                        throw new FormatException();
                    }
                    await _recordService.AddRecordAsync(new Record(amount, selectedCategory.Id, selectedDate, descriptionProperty));
                }
                Clear();
            }
            catch (Exception)
            {
                MessageBox.Show("Please, enter a valid amount.", "Warning: Error Detected in Input amount", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Visibility
        private void UpdateVisibility()
        {

            if (isSavingsChecked || isBalanceChecked)
            {
                VisibilityMainInform = Visibility.Collapsed;
            }
            else { VisibilityMainInform = Visibility.Visible; }

            if (isSavingsChecked)
            {
                VisibilityBalance = Visibility.Collapsed;
            }
            else { VisibilityBalance = Visibility.Visible; }

            if (isBalanceChecked)
            {
                VisibilitySavings = Visibility.Collapsed;
            }
            else { VisibilitySavings = Visibility.Visible; }

            // Update all 
            OnPropertyChanged(nameof(VisibilityMainInform));
            OnPropertyChanged(nameof(VisibilitySavings));
            OnPropertyChanged(nameof(VisibilityBalance));
        }
        #endregion

        #region Back
        private async Task OnBack(string param)
        {
            _navigationService.Navigate(ViewID.DashboardView);
        }
        #endregion

        #region Text Check & Add Check

        private void CheckNumericInput(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                foreach (char ch in input)
                {
                    if (!char.IsDigit(ch) && ch != '-' && ch != ',')
                    {
                        return;
                    }
                }
            }
            amountTextProperty = input;
        }

        private bool CheckAddInput(string amount)
        {
            if (string.IsNullOrEmpty(amount))
            {
                MessageBox.Show("Please, enter a valid amount.", "Warning: Error Detected in Input amount", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!isSavingsChecked && !isBalanceChecked)
            {
                if (!SelectedDate.HasValue)
                {
                    MessageBox.Show("Please, select the date of your record", "Warning: Error Detected in Input Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (SelectedCategory == null)
                {
                    MessageBox.Show("Please, select the type of your record", "Warning: Error Detected in Input Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Clear interface 
        private void Clear()
        {
            IsSavingsChecked = false;
            IsBalanceChecked = false;
            AmountTextProperty = "0";
            DescriptionProperty = string.Empty;
            SelectedDate = DateTime.Now;

            if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }

            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(AmountTextProperty));
            OnPropertyChanged(nameof(DescriptionProperty));
            OnPropertyChanged(nameof(SelectedDate));
        }
        #endregion
    }
}
