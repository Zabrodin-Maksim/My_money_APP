using My_money.Model;
using My_money.Services.IServices;

namespace My_money.Services
{
    public class UserSessionService : IUserSessionService
    {
        public User? CurrentUser {  get; private set; }
        public HouseholdMember? CurrentHouseholdMember { get; private set; }

        public bool IsAuthenticated => CurrentUser != null;

        public void EndSession()
        {
            CurrentUser = null;
            CurrentHouseholdMember = null;
        }

        public void StartSession(User user, HouseholdMember householdMember)
        {
            CurrentUser = user;
            CurrentHouseholdMember = householdMember;
        }
    }
}
