namespace My_money.ViewModel
{
    public class HouseholdMemberListItem
    {
        public int MemberId { get; init; }
        public int UserId { get; init; }
        public required string DisplayName { get; init; }
        public required string Email { get; init; }
        public required string Role { get; init; }
        public int CanManageBudget { get; init; }
        public int CanManageMembers { get; init; }
    }
}
