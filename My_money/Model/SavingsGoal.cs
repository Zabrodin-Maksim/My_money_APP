using My_money.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Model
{
    [Serializable]
    public class SavingsGoal : ViewModelBase
    {
        private string goalName;
        public string GoalName { get { return goalName; } set { goalName = value; } }

        private int have;
        public int Have { get { return have; } set { have = value; CalculPercent(); } }

        private int need;
        public int Need { get { return need; } set { need = value; CalculPercent(); } }

        private float percent;
        public float Percent
        {
            get { return percent; }
            set { SetProperty(ref percent, value); }
        }

        public SavingsGoal() { }

        public SavingsGoal(string goalName, int have, int need)
        {
            this.goalName = goalName;
            this.have = have;
            this.need = need;
        }

        private void CalculPercent()
        {
            if (have != 0 && need != 0)
            {
                Percent = ((float)have / Need) * 100f;
            }
            else
            {
                Percent = 0.0f;
            }
        }
    }
}
