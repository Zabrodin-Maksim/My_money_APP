namespace My_money.Model
{
    public class BudgetCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Plan { get; set; }
        public decimal? Spend { get; set; }
    }
}
