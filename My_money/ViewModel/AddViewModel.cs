using My_money.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        public event Action<Record> RecordAdded;
        public event Action<int> SavingsAdded;
        public event Action<int> BalanceAdded;
        public event Action<string> Back;

        // Visibility for Type and Date
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


        //Properties
        private string costTextProperty;
        public string CostTextProperty
        {
            get { return costTextProperty; }
            set { CheckNumericInput(value); }
        }


        private DateTime? selectedDate = DateTime.Now;
        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set { selectedDate = value; }
        }


        private ObservableCollection<string> types;
        public ObservableCollection<string> Types 
        { 
            get { return types; } 

            set 
            {
                SetProperty(ref types, value);
            } 
        }


        private string selectedType;
        public string SelectedType
        {
            get { return selectedType; }
            set { selectedType = value; }
        }



        public AddViewModel() {
            AddCommand = new MyICommand<object>(OnAdd);
            BackCommand = new MyICommand<string>(OnBack);

            Types = TypesName.Values;

            SelectedType = Types[0];
        }

        #region Commands
        public MyICommand<object> AddCommand { get; private set; }
        public MyICommand<string> BackCommand { get; private set; }
        #endregion

        #region ADD
        private void OnAdd(object parametr)
        {
            // Add in Savings
            if (isSavingsChecked)
            {
                if (!CheckAddInput(costTextProperty))
                {
                    return;
                }

                SavingsAdded?.Invoke(int.Parse(costTextProperty));
            }
            // Add in Balance
            else if (isBalanceChecked)
            {
                if (!CheckAddInput(costTextProperty))
                {
                    return;
                }

                BalanceAdded?.Invoke(int.Parse(costTextProperty));
            }
            // Add in Records
            else
            {
                if (!CheckAddInput(costTextProperty))
                {
                    return;
                }

                Record newRecord = new Record(int.Parse(costTextProperty), selectedDate, selectedType);

                RecordAdded?.Invoke(newRecord);
            }
            Clear();
        }
        #endregion

        #region Visibility
        private void UpdateVisibility()
        {

            if(isSavingsChecked || isBalanceChecked)
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
        private void OnBack(string param)
        {
            Clear();
            Back?.Invoke(param);
        }
        #endregion

        #region Text Check & Add Check

        private void CheckNumericInput(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                foreach (char ch in input)
                {
                    if (!char.IsDigit(ch))
                    {
                        return;
                    }
                }
            }
            costTextProperty = input;
        }

        private bool CheckAddInput(string cost)
        {
            if (string.IsNullOrEmpty(cost))
            {
                MessageBox.Show("Please, enter a valid cost.", "Warning: Error Detected in Input cost", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!isSavingsChecked && !isBalanceChecked)
            {
                if (!SelectedDate.HasValue)
                {
                    MessageBox.Show("Please, select the date of your record", "Warning: Error Detected in Input Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (SelectedType == null)
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
            SelectedType = Types[0];
        }
        #endregion
    }
}
