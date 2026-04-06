using My_money.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IUserRepository
    {
        string ConnectionString { get; }
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<int> AddAsync(User user);
        Task<int> AddAsync(User user, SQLiteConnection connection, SQLiteTransaction transaction);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
