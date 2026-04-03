namespace My_money.Model
{
    public class Household
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required int CreatedByUserId { get; set; }
    }
}
