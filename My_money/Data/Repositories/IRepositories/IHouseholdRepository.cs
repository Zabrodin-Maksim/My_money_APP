using My_money.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IHouseholdRepository
    {
        Task<List<Household>> GetAllAsync();
        Task<Household?> GetByIdAsync(int id);
        Task<int> AddAsync(Household household);
        Task<int> AddAsync(Household household, SQLiteConnection connection, SQLiteTransaction transaction);
        Task UpdateAsync(Household household);
        Task DeleteAsync(int id);
    }
}
