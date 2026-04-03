using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IHouseholdMemberRepository
    {
        Task<List<HouseholdMember>> GetAllAsync();
        Task<HouseholdMember?> GetByIdAsync(int id);
        Task<int> AddAsync(HouseholdMember member);
        Task UpdateAsync(HouseholdMember member);
        Task DeleteAsync(int id);
    }
}
