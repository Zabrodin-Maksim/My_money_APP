namespace My_money.Model
{
    public class HouseholdFinance
    {
        public int Id { get; set; }
        public required int HouseholdId { get; set; }
        public decimal? Savings { get; set; }
        public decimal? Balance { get; set; }
    }
}
