using My_money.Model;

namespace My_money.Services.IServices
{
    public interface IUserSessionService
    {
        User? CurrentUser { get; }
        HouseholdMember? CurrentHouseholdMember { get; }
        bool IsAuthenticated { get; }

        void StartSession(User user, HouseholdMember householdMember);
        void EndSession();
    }
}
