using My_money.Model;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IUserFinanceRepository
    {
        Task<UserFinance?> GetByUserIdAsync(int userId);
        Task<UserFinance?> GetByUserIdAsync(int userId, SQLiteConnection connection, SQLiteTransaction transaction);
        Task<int> AddAsync(UserFinance userFinance);
        Task UpdateAsync(UserFinance userFinance);
        Task UpdateAsync(UserFinance userFinance, SQLiteConnection connection, SQLiteTransaction transaction);
        Task DeleteAsync(int id);
    }
}
