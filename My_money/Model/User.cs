namespace My_money.Model
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string DisplayName { get; set; }
        public required int IsActive { get; set; }
    }
}
