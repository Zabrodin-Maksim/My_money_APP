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
        public event Action<int> BankAdded;
        public event Action<string> BackM;

        private Visibility visibilityByBank;
        public Visibility VisibilityByBank { get { return visibilityByBank; } set { visibilityByBank = value; } }


        private bool isCheckBoxChecked;
        public bool IsCheckBoxChecked
        {
            get { return isCheckBoxChecked; }
            set
            {
                isCheckBoxChecked = value;
                UpdateVisibility();
            }
        }


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
        }

        #region Commands
        public MyICommand<object> AddCommand { get; private set; }
        public MyICommand<string> BackCommand { get; private set; }
        #endregion

        #region ADD
        private void OnAdd(object parametr)
        {
            if (isCheckBoxChecked)
            {
                if (!CheckAddInput(costTextProperty))
                {
                    return;
                }

                BankAdded?.Invoke(int.Parse(costTextProperty));
            }
            else
            {
                if (!CheckAddInput(costTextProperty))
                {
                    return;
                }

                Record newRecord = new Record(int.Parse(costTextProperty), selectedDate, selectedType);

                RecordAdded?.Invoke(newRecord);
            }
        }
        #endregion

        #region Visibility
        private void UpdateVisibility()
        {
            if (isCheckBoxChecked)
            {
                VisibilityByBank = Visibility.Collapsed;
            }
            else
            {
                VisibilityByBank = Visibility.Visible;
            }
            OnPropertyChanged(nameof(VisibilityByBank));
        }
        #endregion

        #region Back
        private void OnBack(string param)
        {
            BackM?.Invoke(param);
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
            if (!isCheckBoxChecked)
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

    }
}
