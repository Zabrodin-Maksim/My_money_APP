using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IHouseholdMemberService
    {
        Task<List<HouseholdMember>> GetAllHouseholdMembersByHouseholdIdAsync();
        Task<HouseholdMember?> GetHouseholdMemberByIdAsync(int id);
        Task<HouseholdMember?> GetHouseholdMemberByAuthenticatedUserAsync();
        Task<int> AddHouseholdMemberAsync(HouseholdMember member);
        Task UpdateHouseholdMemberAsync(HouseholdMember member);
        Task DeleteHouseholdMemberAsync(int id);
    }
}
