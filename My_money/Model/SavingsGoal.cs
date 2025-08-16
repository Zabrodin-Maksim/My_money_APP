namespace My_money.Model
{
    public class SavingsGoal
    {
        public int Id { get; set; }
        public string GoalName { get; set; }
        public decimal? Have { get; set; }
        public decimal? Need { get; set; }
    }
}
