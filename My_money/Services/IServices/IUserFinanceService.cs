using My_money.Enums;
using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IUserFinanceService
    {
        /// <summary>
        /// Get list for household member type Child in personal finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="UserFinance"/> objects created by the authenticated user within the specified household.</returns>
        Task<List<UserFinance>> GetAllByHouseholdAndCreatedByAsync(int householdId);
        /// <summary>
        /// Get list for Household in shared finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="UserFinance"/> objects associated with the specified household.</returns>
        Task<List<UserFinance>> GetAllByHouseholdIdAsync(int householdId);
        /// <summary>
        /// Get list in personal finances
        /// </summary>
        /// <returns>A list of <see cref="UserFinance"/> objects associated with the authenticated user.</returns>
        Task<List<UserFinance>> GetAllByOwnerAsync();

        Task<UserFinance?> GetByUserIdAsync(int userId);
        Task<int> AddUserFinanceAsync(UserFinance userFinance);
        Task ApplyExpenseAsync(decimal amount, IncomeTarget target);
        Task ApplyIncomeAsync(decimal amount, IncomeTarget target);
        Task UpdateUserFinanceAsync(decimal? savings, decimal? balance);
        Task DeleteUserFinance(int id);
    }
}
