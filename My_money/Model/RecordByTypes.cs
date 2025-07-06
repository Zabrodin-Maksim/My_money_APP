using System.Windows;

namespace My_money.Model
{
    public class RecordByTypes
    {

        #region Properties

        private string name;
        public string Name { get { return name; } 
            set 
            {
                CheckNameExist(value);
            } 
        }

        private float spend;
        public float Spend { get { return spend; } 
            set 
            {
                SetProperty(ref spend, value);
                CalculRemaining();
            } 
        }

        private float plan;
        public float Plan { get { return plan; } set 
            { 
                SetProperty(ref plan, value); 
            } 
        }

        private float planByDatePeriod;
        public float PlanByDatePeriod { get { return planByDatePeriod; }
            set
            {
                SetProperty(ref planByDatePeriod, CalculPlanByPeriod(value));
            }
        }

        private float remaining;
        public float Remaining { get { return remaining; } set { SetProperty(ref remaining, value); } }

        
        #endregion

        public RecordByTypes() { }

        public RecordByTypes(string name, float plan)
        {
            this.name = name;
            this.plan = plan;
        }

        private void CalculRemaining()
        {
            Remaining = 0;
            Remaining = planByDatePeriod - spend;
        }

        private float CalculPlanByPeriod (float period)
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
            // if edit type is "Other"
            if (name == "Other")
            {
                MessageBox.Show("You can't edit name of type Other!", "Warning: Can't delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                for (int i = 0; i < TypesName.Values.Count; i++)
                {
                    // if try add exists type
                    if (newName == TypesName.Values[i] && ViewModelBase.flagStartProg)
                    {
                        MessageBox.Show("This type (" + newName + ") already exists.", "Warning: Type already exists.", MessageBoxButton.OK, MessageBoxImage.Warning);
                        newName = "Fuck";
                    }else
                    //if edit type
                    if (TypesName.Values[i] == name)
                    {
                        TypesName.Values[i] = newName;
                    }
                }
                //if new type (add name in list of TypesName)
                if (!TypesName.Values.Contains(newName) && newName != "Fuck")
                {
                    TypesName.Values.Add(newName);
                }
            }
            if (newName != "Fuck")
            {
                SetProperty(ref name, newName);
            }
        }
    }
}
