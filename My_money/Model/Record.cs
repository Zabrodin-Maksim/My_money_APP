using System;

namespace My_money.Model
{
    [Serializable]
    public class Record 
    {
        private float cost;
        private string type;
        private DateTime? dateTimeOccurred;

        public float Cost { get { return cost; }
            set { 
                cost = value;
            }
        }
        public string Type {  get { return type; } 
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
        public Record(float cost, DateTime? dateTime, String type)
        {
            this.cost = cost;
            this.dateTimeOccurred = dateTime;
            this.type = type;
        }
    }
}
