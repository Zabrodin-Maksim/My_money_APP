namespace My_money.Model
{
    public class SavingsGoal
    {
        public int Id { get; set; }
        public required string GoalName { get; set; }
        public decimal Have { get; set; }
        public decimal Need { get; set; }
        public int HouseholdId { get; set; }
        public int OwnerUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public required string Scope { get; set; }

        //ToDO: ADD Percent
        public decimal Percent { get => Need == 0 ? 0 : Have / Need * 100; }

        public SavingsGoal() { }
        public SavingsGoal(string goalName, decimal have, decimal need, string scope)
        {
            GoalName = goalName ?? "Enter Name";
            Have = have;
            Need = need;
            Scope = scope;
        }
    }
}
