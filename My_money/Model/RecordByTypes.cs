using My_money.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace My_money.Model
{
    
    [Serializable]
    public class RecordByTypes : ViewModelBase
    {
        #region Properties

        private string name;
        public string Name { get { return name; } 
            set 
            {
                CheckNameExist(value);
                SetProperty(ref name, value); 
            } 
        }

        private int spend;
        public int Spend { get { return spend; } 
            set 
            {
                SetProperty(ref spend, value);
                CalculRemaining();
            } 
        }

        private int plan;
        public int Plan { get { return plan; } set 
            { 
                SetProperty(ref plan, value); 
            } 
        }

        private int planByDatePeriod;
        public int PlanByDatePeriod { get { return planByDatePeriod; }
            set
            {
                SetProperty(ref planByDatePeriod, CalculPlanByPeriod(value));
            }
        }

        private int remaining;
        public int Remaining { get { return remaining; } set { SetProperty(ref remaining, value); } }

        #endregion

        public RecordByTypes() { }

        public RecordByTypes(string name, int plan)
        {
            this.name = name;
            this.plan = plan;
        }

        private void CalculRemaining()
        {
            Remaining = 0;
            Remaining = planByDatePeriod - spend;
        }

        private int CalculPlanByPeriod (int period)
        {
            switch (period)
            {
                // Day
                case 0:
                    return plan / 30;
                // Month
                case 1:
                    return plan;
                // Year
                case 2:
                    return plan * 12;
                default:
                    return plan;
            }
        }

        private void CheckNameExist(string newName)
        {
            //if edit type
            for (int i = 0; i < TypesName.Values.Count; i++)
            {
                if (TypesName.Values[i] == name)
                {
                    TypesName.Values[i] = newName;
                }
            }
            //if new type
            if (!TypesName.Values.Contains(newName))
            {
                TypesName.Values.Add(newName);
            }
            
        }
    }
}
