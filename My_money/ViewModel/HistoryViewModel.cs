using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    // TODO: ПРИ ИЗМЕНЕНИИ ЭЛЕМЕНТА В UI обновлять в базе
    public class HistoryViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<Record> records;
        public ObservableCollection<Record> Records
        {
            get { return records; }
            set
            {
                SetProperty(ref records, value);
            }
        }

        private Record selectedItem;
        public Record SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
            }
        }

        private double selectedSort;
        public double SelectedSort
        {
            get { return selectedSort; }
            set
            {
                selectedSort = value;
                SortingRecords();
            }
        }
        #endregion

        public MyICommand<object> DeleteCommand { get; set; }

        #region Dependency Injection Services
        private readonly IRecordService _recordService;
        #endregion

        public HistoryViewModel(IRecordService recordService)
        {
            _recordService = recordService;

            _ = LoadDataAsync();

            DeleteCommand = new MyICommand<object>(OnDelete);
        }

        private async Task LoadDataAsync()
        {
            var recordsFromDb = await _recordService.GetAllRecordsAsync();
            Records = new ObservableCollection<Record>(recordsFromDb);
            SortingRecords();
        }

        private async Task OnDelete(object obj)
        {
            try
            {
                if (SelectedItem != null)
                {
                    await _recordService.DeleteRecordAsync(SelectedItem);
                    await LoadDataAsync();
                }
                else
                {
                    throw new InvalidOperationException("No item selected for deletion.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
        }

        public void SortingRecords()
        {
            switch (SelectedSort)
            {
                /// 0 - sort by date, 1 - sort by cost
                case 0:
                    Records = new ObservableCollection<Record>(Records.OrderByDescending(item => item.DateTimeOccurred));
                    break;
                case 1:
                    Records = new ObservableCollection<Record>(Records.OrderByDescending(item => item.Cost));
                    break;
            }

        }
    }
}
