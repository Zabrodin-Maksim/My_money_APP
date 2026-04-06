using My_money.Enums;
using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IRegistrationService
    {
        Task RegisterAdminAndHouseholdAsync(string username, string email, Household household);
        Task RegisterUserAsync(string? passwordHash, string username, string email, HouseholdMemberRole role);
    }
}
