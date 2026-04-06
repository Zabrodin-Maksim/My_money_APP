using My_money.Enums;
using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IUserFinanceService
    {
        Task<UserFinance?> GetByUserIdAsync(int userId);
        Task<int> AddUserFinanceAsync(UserFinance userFinance);
        Task ApplyExpenseAsync(decimal amount, IncomeTarget target);
        Task ApplyIncomeAsync(decimal amount, IncomeTarget target);
        Task UpdateUserFinanceAsync(decimal? savings, decimal? balance);
        Task DeleteUserFinance(int id);
    }
}
