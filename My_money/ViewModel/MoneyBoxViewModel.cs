using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace My_money.ViewModel
{
    public class MoneyBoxViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<SavingsGoal> savingsGoals;
        public ObservableCollection<SavingsGoal> SavingsGoals
        {
            get { return savingsGoals; }
            set { SetProperty(ref savingsGoals, value); }
        }

        public decimal? savingsAmount;

        private decimal? totalSavings;
        public decimal? TotalSavings
        {
            get { return totalSavings; }
            set { SetProperty(ref totalSavings, value); }
        }

        private decimal? notUsedMoney;
        public decimal? NotUsedMoney
        {
            get { return notUsedMoney; }
            set { SetProperty(ref notUsedMoney, value); }
        }

        public SavingsGoal selectedItem { get; set; }
        #endregion

        #region Commands
        public MyICommand<object> AddCommand { get; set; }
        public MyICommand<object> DeleteCommand { get; set; }
        public MyICommand<object> UpdateCommand { get; set; }
        #endregion

        #region Dependency Injection Services
        private readonly ISavingsGoalService _savingsGoalService;
        private readonly IUserFinanceService _userFinanceService;
        #endregion

        public MoneyBoxViewModel(
            ISavingsGoalService savingsGoalService,
            IUserFinanceService userFinanceService
            )
        {
            #region DI Services
            _savingsGoalService = savingsGoalService;
            _userFinanceService = userFinanceService;
            #endregion

            _ = LoadDataAsync();

            AddCommand = new MyICommand<object>(OnAdd);
            DeleteCommand = new MyICommand<object>(OnDelete);
            UpdateCommand = new MyICommand<object>(OnUpdate);
        }

        private async Task LoadDataAsync()
        {
            var userFinance = await _userFinanceService.GetUserFinanceAsync();
            var savingsGoals = await _savingsGoalService.GetAllSavingsGoals();

            savingsAmount = userFinance.Savings ?? 0;
            TotalSavings = savingsAmount;
            SavingsGoals = new ObservableCollection<SavingsGoal>(savingsGoals);

            CalculNotUsedMoney();
        }

        private async Task OnAdd(object par)
        {
            await _savingsGoalService.AddSavingsGoal(new SavingsGoal("Enter Name", 0, 0));
            await LoadDataAsync();
        }

        private async Task OnDelete(object par)
        {
            if (selectedItem != null)
            {
                try
                {
                    if (selectedItem.Have == selectedItem.Need)
                    {
                        await _userFinanceService.AddToSavingsAsync(-selectedItem.Have);
                        await _savingsGoalService.DeleteSavingsGoal(selectedItem.Id);
                    }
                    else
                    {
                        await _userFinanceService.AddToSavingsAsync(selectedItem.Have);
                        await _savingsGoalService.DeleteSavingsGoal(selectedItem.Id);
                    }

                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the savings goal: " + ex.Message, "Error Detected in Delete Savings Goal", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please, select the Item", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task OnUpdate(object par)
        {
            
            if (selectedItem != null)
            {
                try
                {
                    await _savingsGoalService.UpdateSavingsGoal(selectedItem);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    await LoadDataAsync();
                    MessageBox.Show("An error occurred while updating the savings goal: " + ex.Message, "Error Detected in Update Savings Goal", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
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
                NotUsedMoney = savingsAmount - SavingsGoals.Sum(goal => goal.Have);
            }
        }
    }
}
