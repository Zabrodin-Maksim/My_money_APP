using My_money.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        public event Action<Record> RecordAdded;
        public event Action<string> BackM;


        private string costTextProperty;
        public string CostTextProperty
        {
            get { return costTextProperty; }
            set { CheckNumericInput(value); }
        }


        private DateTime? selectedDate;
        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set { selectedDate = value; }
        }


        public List<TypesRecord> Types { get; set; }

        private TypesRecord selectedType;
        public TypesRecord SelectedType
        {
            get { return selectedType; }
            set { selectedType = value; }
        }


        public AddViewModel() {
            AddCommand = new MyICommand<object>(OnAdd);
            BackCommand = new MyICommand<string>(OnBack);

            Types = Enum.GetValues(typeof(TypesRecord)).Cast<TypesRecord>().ToList();
        }

        #region Commands
        public MyICommand<object> AddCommand { get; private set; }
        public MyICommand<string> BackCommand { get; private set; }
        #endregion

        #region ADD
        private void OnAdd(object parametr)
        {
            if (!CheckAddInput(costTextProperty, selectedDate, null))
            {
                return;
            }
            Record newRecord = new Record { Cost = int.Parse(costTextProperty), DateTimeOccurred = selectedDate, Type = selectedType };

            RecordAdded?.Invoke(newRecord);
        }
        #endregion

        private void OnBack(string param)
        {
            BackM?.Invoke(param);
        }

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

        private bool CheckAddInput(string cost, DateTime? dateTime, TypesRecord? typsRecord)
        {
            if (string.IsNullOrEmpty(cost))
            {
                MessageBox.Show("Please enter a valid cost.", "Error Detected in Input cost", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        #endregion

    }
}
