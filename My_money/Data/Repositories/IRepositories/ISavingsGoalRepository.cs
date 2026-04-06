using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface ISavingsGoalRepository
    {
        // Get for Child in personal finances
        Task<List<SavingsGoal>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId);
        // Get for Household in shared finances
        Task<List<SavingsGoal>> GetAllByHouseholdIdAsync(int householdId);
        // Get in personal finances
        Task<List<SavingsGoal>> GetAllByOwnerAsync(int ownerUserId);

        Task<SavingsGoal?> GetByIdAsync(int id);
        Task<int> AddAsync(SavingsGoal goal);
        Task UpdateAsync(SavingsGoal goal);
        Task DeleteAsync(int id);
    }
}
