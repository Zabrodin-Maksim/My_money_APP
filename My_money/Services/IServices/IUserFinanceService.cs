using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IUserFinanceService
    {
        Task<UserFinance> GetUserFinanceAsync();
        Task<int> AddUserFinanceAsync(UserFinance userFinance);
        Task UpdateUserFinanceAsync(decimal savings, decimal balance);
        Task<UserFinance> AddDefaultUserFinance();
    }
}
