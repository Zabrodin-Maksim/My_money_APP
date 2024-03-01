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
    public class Record 
    {

        private int cost;
        private TypesRecord type;
        private DateTime? dateTimeOccurred;

        public int Cost { get { return cost; }
            set { 
                cost = value;
            }
        }

        public TypesRecord Type {  get { return type; } 
            set { 
                type = value;
            } 
        }

        public DateTime? DateTimeOccurred
        {
            get { return dateTimeOccurred; }
            set
            {
                dateTimeOccurred = value;
            }
        }

        public Record() { }

        public Record(int cost, DateTime? dateTime, TypesRecord type)
        {
            this.cost = cost;
            this.dateTimeOccurred = dateTime;
            this.type = type;
        }
    }
}
