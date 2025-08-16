
using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Services
{
    public interface IUserFinanceService
    {
        Task<UserFinance> GetUserFinanceAsync();
        Task AddUserFinanceAsync(UserFinance userFinance);
        Task UpdateUserFinanceAsync(float savings, float balance);
    }
}
