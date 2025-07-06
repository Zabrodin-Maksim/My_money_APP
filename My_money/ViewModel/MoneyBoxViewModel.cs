using Microsoft.VisualBasic;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace My_money.ViewModel
{
    public class MoneyBoxViewModel : ViewModelBase
    {
        public event Action<float> DeleteGoal;


        private ObservableCollection<SavingsGoal> savingsGoals;
        public ObservableCollection<SavingsGoal> SavingsGoals
        {
            get { return savingsGoals; }
            set { SetProperty(ref savingsGoals, value); }
        }

        private float savings; //from Dashboard
        public float Savings { get { return savings; } set { SetProperty(ref savings, value); CalculNotUsedMoney(); } }

        private float notUsedMoney;
        public float NotUsedMoney
        {
            get { return notUsedMoney; }
            set { SetProperty(ref notUsedMoney, value); }
        }

        public SavingsGoal selectedItem { get; 
            set; }

        public MyICommand<object> AddCommand { get; set; }
        public MyICommand<object> DeleteCommand { get; set; }


        public MoneyBoxViewModel(ObservableCollection<SavingsGoal> _savingsGoals, float _savings)
        {
            savings = _savings;
            SavingsGoals = _savingsGoals;

            AddCommand = new MyICommand<object>(OnAdd);
            DeleteCommand = new MyICommand<object>(OnDelete);

            CalculNotUsedMoney();

            SubscribeToPropertyChangeEvents();
        }

        private void SubscribeToPropertyChangeEvents()
        {
            if (SavingsGoals.Count > 0)
            {
                foreach (var goal in SavingsGoals)
                {
                    goal.PropertyChanged += Goal_PropertyChanged;
                }
            }
        }
        
        private void Goal_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Have")
            {
                CalculNotUsedMoney();
            }
        }


        private void OnAdd(object par)
        {
            SavingsGoals.Add(new SavingsGoal("Enter Name", 0, 0));

            SubscribeToPropertyChangeEvents();
        }

        private void OnDelete(object par)
        {
            if(selectedItem != null)
            {
                if(selectedItem.Percent >= 100.0f)
                {
                    savings -= selectedItem.Have;
                    DeleteGoal.Invoke(selectedItem.Have);
                }
                selectedItem.PropertyChanged -= Goal_PropertyChanged;
                
                SavingsGoals.Remove(selectedItem);

                CalculNotUsedMoney();
            }
            else
            {
                MessageBox.Show("Please, select the Item", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void CalculNotUsedMoney()
        {
            if (SavingsGoals != null)
            {
                NotUsedMoney = savings - SavingsGoals.Sum(goal => goal.Have);
            }
        }
    }
}
