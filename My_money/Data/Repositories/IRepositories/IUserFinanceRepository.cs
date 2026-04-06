using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IUserFinanceRepository
    {
        // Get for Child in personal finances
        Task<List<UserFinance>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId);
        // Get for Household in shared finances
        Task<List<UserFinance>> GetAllByHouseholdIdAsync(int householdId);
        // Get in personal finances
        Task<List<UserFinance>> GetAllByOwnerAsync(int ownerUserId);

        Task<UserFinance?> GetByUserIdAsync(int userId);
        Task<int> AddAsync(UserFinance userFinance);
        Task UpdateAsync(UserFinance userFinance);
        Task DeleteAsync(int id);
    }
}
