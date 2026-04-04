namespace My_money.Model
{
    public class UserFinance
    {
        public int Id { get; set; }
        public decimal Savings { get; set; }
        public decimal Balance { get; set; }
        public int UserId { get; set; }
    }
}
