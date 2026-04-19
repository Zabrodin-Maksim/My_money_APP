using My_money.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IBudgetCategoryRepository
    {
        // Get for Child in personal finances
        Task<List<BudgetCategory>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId);
        // Get for Household in shared finances
        Task<List<BudgetCategory>> GetAllByHouseholdIdAsync(int householdId);
        // Get in personal finances
        Task<List<BudgetCategory>> GetAllByOwnerAsync(int ownerUserId);

        Task<BudgetCategory?> GetByIdAsync(int id);
        Task<int> AddAsync(BudgetCategory category);
        Task<int> AddAsync(BudgetCategory category, SQLiteConnection connection, SQLiteTransaction transaction);
        Task UpdateAsync(BudgetCategory category);
        Task DeleteAsync(int id);

        Task<BudgetCategory?> GetByNameAsync(string name, int householdId, int? ownerUserId, string scope);
    }
}
