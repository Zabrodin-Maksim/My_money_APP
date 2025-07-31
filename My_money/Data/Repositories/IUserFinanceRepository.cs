using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public interface IUserFinanceRepository
    {
        Task<UserFinance> GetAsync();
        Task AddAsync(UserFinance userFinance);
        Task UpdateAsync(UserFinance userFinance);
        Task DeleteAsync(int id);
    }
}
