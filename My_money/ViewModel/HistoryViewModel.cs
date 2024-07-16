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
    public class HistoryViewModel : ViewModelBase
    {
        private ObservableCollection<Record> records;
        public ObservableCollection<Record> Records {  get { return records; } 
            set 
            {
                SetProperty(ref records, value);
            } 
        }

        public event Action<float> BalanceBack;

        private Record selectedItem;
        public Record SelectedItem
        {
            get { return selectedItem; } 
            set 
            { 
                selectedItem = value;
            }
        }

        private float selectedSort;
        public float SelectedSort { get { return selectedSort; }
            set 
            { 
                selectedSort = value;
                SortingRecords();
            }
        }

        List<Record> sortedList;

        public MyICommand<object> DeleteCommand {  get; set; }

        public HistoryViewModel(ObservableCollection<Record> Records) 
        {
            this.Records = Records;

            DeleteCommand = new MyICommand<object>(OnDelete);
        }

        private void OnDelete(object obj)
        {
            if(selectedItem != null)
            {
                BalanceBack.Invoke(SelectedItem.Cost);
                Records.Remove(SelectedItem);
            }
            else
            {
                MessageBox.Show("Please, select the Item", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void SortingRecords()
        {
            switch (SelectedSort)
            {
                case 0:
                    sortedList = Records.OrderByDescending(item => item.DateTimeOccurred).ToList();
                    Records.Clear();
                    foreach (var item in sortedList)
                    {
                        Records.Add(item);
                    }
                    break;
                case 1:
                    sortedList = Records.OrderByDescending(item => item.Cost).ToList();
                    Records.Clear();
                    foreach (var item in sortedList)
                    {
                        Records.Add(item);
                    }
                    break;
            }
            
        }
    }
}
