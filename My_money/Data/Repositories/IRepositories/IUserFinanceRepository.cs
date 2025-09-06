using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IUserFinanceRepository
    {
        Task<UserFinance?> GetAsync();
        Task<int> AddAsync(UserFinance userFinance);
        Task UpdateAsync(UserFinance userFinance);
        Task DeleteAsync(int id);
    }
}
