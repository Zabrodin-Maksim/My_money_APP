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
        //TODO: Style and button delete
        private ObservableCollection<Record> records;
        public ObservableCollection<Record> Records {  get { return records; } 
            set 
            { 
                records = value; 
            } 
        }

        private MainViewModel _mainViewModel;
        public HistoryViewModel(ObservableCollection<Record> Records) 
        {
            
            this.Records = Records;
        }
    }
}
