using My_money.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        private ObservableCollection<Record> records;
        public ObservableCollection<Record> Records {  get { return records; } 
            set 
            { 
                records = value; 
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

        private int selectedSort;
        public int SelectedSort { get { return selectedSort; }
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
            Records.Remove(SelectedItem);
        }

        private void SortingRecords()
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
