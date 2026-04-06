using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IHouseholdService
    {
        Task<List<Household>> GetAllHouseholdsAsync();
        Task<Household?> GetHouseholdByAuthenticatedUserAsync();
        Task<Household?> GetHouseholdByIdAsync(int id);
        Task<int> AddHouseholdAsync(Household household);
        Task UpdateHouseholdAsync(Household household);
        Task DeleteHouseholdAsync(int id);
    }
}
