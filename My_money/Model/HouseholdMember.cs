namespace My_money.Model
{
    public class HouseholdMember
    {
        public int Id { get; set; }
        public required int HouseholdId { get; set; }
        public required int UserId { get; set; }
        public required string Role { get; set; }
        public required int CanViewShared { get; set; }
        public required int CanManageBudget { get; set; }
        public required int CanManageMembers { get; set; }
    }
}
