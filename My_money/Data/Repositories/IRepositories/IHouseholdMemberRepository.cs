using My_money.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IHouseholdMemberRepository
    {
        Task<List<HouseholdMember>> GetAllByHouseholdIdAsync(int householdId);
        Task<HouseholdMember?> GetByIdAsync(int id);
        Task<HouseholdMember?> GetByUserIdAsync(int userId);
        Task<int> AddAsync(HouseholdMember member);
        Task<int> AddAsync(HouseholdMember member, SQLiteConnection connection, SQLiteTransaction transaction);
        Task UpdateAsync(HouseholdMember member);
        Task DeleteAsync(int id);
    }
}
