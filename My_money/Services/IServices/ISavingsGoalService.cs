using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface ISavingsGoalService
    {
        /// <summary>
        /// Get list for household member type Child in personal finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="SavingsGoal"/> objects created by the authenticated user within the specified household.</returns>
        Task<List<SavingsGoal>> GetAllByHouseholdAndCreatedByAsync(int householdId);
        /// <summary>
        /// Get list for Household in shared finances
        /// </summary>
        /// <param name="householdId">The ID of the household.</param>
        /// <returns>A list of <see cref="SavingsGoal"/> objects associated with the specified household.</returns>
        Task<List<SavingsGoal>> GetAllByHouseholdIdAsync(int householdId);
        /// <summary>
        /// Get list in personal finances
        /// </summary>
        /// <returns>A list of <see cref="SavingsGoal"/> objects associated with the authenticated user.</returns>
        Task<List<SavingsGoal>> GetAllByOwnerAsync();

        Task<SavingsGoal?> GetSavingsGoal(int id);
        Task<int> AddSavingsGoal(SavingsGoal goal);
        Task UpdateSavingsGoal(SavingsGoal goal);
        Task DeleteSavingsGoal(int id);
    }
}
