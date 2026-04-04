namespace My_money.Model
{
    public class BudgetCategory
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Plan { get; set; }
        public int HouseholdId { get; set; }
        public int? OwnerUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public required string Scope { get; set; }

        // Calculated property for spend by period
        public decimal? SpendByPeriod { get; set; } = 0;

        // Calculated property for remaining budget by period
        public decimal? RemainingByPeriod { get; set; } = 0;

        // Calculated property
        public decimal? PlanByPeriod { get; set; } = 0;
    }
}
