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
    public class MoneyBoxViewModel : ViewModelBase
    {
        private ObservableCollection<SavingsGoal> savingsGoals;
        public ObservableCollection<SavingsGoal> SavingsGoals
        {
            get { return savingsGoals; }
            set { SetProperty(ref savingsGoals, value); }
        }

        private string notUsedMoney;
        public string NotUsedMoney
        {
            get { return "Not-used money: " + notUsedMoney; }
            set { notUsedMoney = value; }
        }

        public SavingsGoal selectedItem { get; set; }

        public MyICommand<object> AddCommand { get; set; }
        public MyICommand<object> DeleteCommand { get; set; }


        public MoneyBoxViewModel(ObservableCollection<SavingsGoal> _savingsGoals)
        {
            SavingsGoals = _savingsGoals;

            AddCommand = new MyICommand<object>(OnAdd);
            DeleteCommand = new MyICommand<object> (OnDelete);
        }


        private void OnAdd(object par)
        {
            SavingsGoals.Add(new SavingsGoal("Enter Name", 0, 0));
        }

        private void OnDelete(object par)
        {
            if(selectedItem != null)
            {
                SavingsGoals.Remove(selectedItem);
            }
            else
            {
                MessageBox.Show("Please, select the Item", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
