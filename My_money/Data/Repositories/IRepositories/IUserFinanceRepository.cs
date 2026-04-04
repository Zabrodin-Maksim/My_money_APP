using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IUserFinanceRepository
    {
        Task<List<UserFinance>> GetAllAsync();
        Task<UserFinance?> GetByIdAsync(int id);
        Task<int> AddAsync(UserFinance userFinance);
        Task UpdateAsync(UserFinance userFinance);
        Task DeleteAsync(int id);
    }
}
