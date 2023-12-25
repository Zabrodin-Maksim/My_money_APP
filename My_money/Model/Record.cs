using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Model
{
    [Serializable]
    public class Record : INotifyPropertyChanged
    {

        private int cost;
        private TypsRecord typ;
        private DateTime dateTimeOccurred;

        public int Cost { get { return cost; }
            set { 
                cost = value;
                RaisePropertyChanged("Cost");
            }
        }

        public TypsRecord Typ {  get { return typ; } 
            set { 
                typ = value;
                RaisePropertyChanged("Typ");
            } 
        }

        public DateTime DateTimeOccurred
        {
            get { return dateTimeOccurred; }
            set
            {
                dateTimeOccurred = value;
                RaisePropertyChanged("DateTimeOccurred");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
