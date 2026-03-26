using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace My_money.ViewModel
{
    public class PlanViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<BudgetCategory> budgetCategories;
        public ObservableCollection<BudgetCategory> BudgetCategories { get { return budgetCategories; } set { SetProperty(ref budgetCategories, value); } }
        #endregion

        public BudgetCategory selectedItem { get; set; }

        #region Commands
        public MyICommand<object> AddCommand { get; set; }
        public MyICommand<object> DeleteCommand { get; set; }
        #endregion

        #region Dependency Injection Services
        private readonly IBudgetCategoryService _budgetCategoryService;
        #endregion

        public PlanViewModel(IBudgetCategoryService budgetCategoryService)
        {
            _budgetCategoryService = budgetCategoryService;

            _ = LoadDataAsync();

            AddCommand = new MyICommand<object>(OnAdd);
            DeleteCommand = new MyICommand<object>(OnDelete);
        }

        private async Task LoadDataAsync()
        {
            BudgetCategories = new ObservableCollection<BudgetCategory>(await _budgetCategoryService.GetAllBudgetCategoriesAsync());
        }

        private async Task OnDelete(object par)
        {
            try
            {
                if (selectedItem != null)
                {
                    await _budgetCategoryService.DeleteBudgetCategoryAsync(selectedItem);
                    MessageBox.Show("All records of type " + selectedItem.Name + " have been moved under the 'Other' type.", "Information: Successful deletion of the type" + selectedItem.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    throw new ArgumentNullException("Please, select the Item");
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Detected on Delete action", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }

        private async Task OnAdd(object par)
        {
            try
            {
                await _budgetCategoryService.AddBudgetCategoryAsync(new BudgetCategory() { Name = "New category", Plan = 0m, Spend = 0m});
                await LoadDataAsync();
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message, "Error Detected on Add action", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
