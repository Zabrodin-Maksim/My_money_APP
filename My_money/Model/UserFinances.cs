namespace My_money.Model
{
    public class UserFinance
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public decimal Savings { get; set; }
        public decimal Balance { get; set; }
    }
}
