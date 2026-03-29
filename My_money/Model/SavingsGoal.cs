namespace My_money.Model
{
    public class SavingsGoal
    {
        public int Id { get; set; }
        public string GoalName { get; set; }
        public decimal Have { get; set; }
        public decimal Need { get; set; }

        //ToDO: ADD Percent

        public SavingsGoal() { }
        public SavingsGoal(string goalName, decimal have, decimal need)
        {
            GoalName = goalName ?? "Enter Name";
            Have = have;
            Need = need;
        }
    }
}
