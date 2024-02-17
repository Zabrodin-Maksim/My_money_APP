using My_money.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.ViewModel
{
    public class AddViewModel : ViewModelBase
    {
        public event Action<Record> RecordAdded;

        public AddViewModel() {
            AddCommand = new MyICommand<object>(OnAdd);
        }

        #region Commands
        public MyICommand<object> AddCommand { get; private set; }
        #endregion

        #region ADD
        private void OnAdd(object parametr)
        {
            Record newRecord= new Record { Cost = 1};

            RecordAdded?.Invoke(newRecord);
        }
        #endregion

        #region Text Check
        private string costTextProperty;
        public string CostTextProperty
        {
            get { return costTextProperty; }
            set { CheckNumericInput(value); }
        }

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
        #endregion

    }
}
