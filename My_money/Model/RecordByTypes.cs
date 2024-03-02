using My_money.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Model
{
    // TODO: СДЕЛАТЬ ЗАВИСИМОСТЬ CalculRemaining ОТ ВЫБРОННОГО ПЕРИОДА ВРЕМЕНИ
    [Serializable]
    public class RecordByTypes : ViewModelBase
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }

        private int spend;
        public int Spend { get { return spend; } 
            set 
            {
                SetProperty(ref spend, value);
                CalculRemaining();
            } 
        }

        private int plan;
        public int Plan { get { return plan; } set { plan = value; SetProperty(ref plan, value); } }

        private int remaining;
        public int Remaining { get { return remaining; } set { SetProperty(ref remaining, value); } }

        public RecordByTypes() { }

        public RecordByTypes(String name, int plan)
        {
            this.name = name;
            this.plan = plan;
        }

        private void CalculRemaining()
        {
            Remaining = 0;
            Remaining = plan - spend;
        }
    }
}
