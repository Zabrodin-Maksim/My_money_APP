using My_money.Model;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IHouseholdFinanceRepository
    {
        Task<HouseholdFinance?> GetByIdAsync(int id);
        Task<HouseholdFinance?> GetByHouseholdIdAsync(int householdId);
        Task<HouseholdFinance?> GetByHouseholdIdAsync(int householdId, SQLiteConnection connection, SQLiteTransaction transaction);
        Task<int> AddAsync(HouseholdFinance finance);
        Task<int> AddAsync(HouseholdFinance finance, SQLiteConnection connection, SQLiteTransaction transaction);
        Task UpdateAsync(HouseholdFinance finance);
        Task UpdateAsync(HouseholdFinance finance, SQLiteConnection connection, SQLiteTransaction transaction);
        Task DeleteAsync(int id);
    }
}
