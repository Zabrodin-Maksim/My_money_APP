namespace My_money.Model
{
    public class BudgetCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Plan { get; set; }
        public decimal? Spend { get; set; }

        // Calculated property for spend by period
        public decimal? SpendByPeriod { get; set; } = 0;

        // Calculated property for remaining budget by period
        public decimal? RemainingByPeriod { get; set; } = 0;

        // Calculated property
        public decimal? PlanByPeriod { get; set; } = 0;
    }
}
