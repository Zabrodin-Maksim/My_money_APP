using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public interface IBudgetCategoryRepository
    {
        Task<List<BudgetCategory>> GetAllAsync();
        Task<BudgetCategory> GetByIdAsync(int id);
        Task AddAsync(BudgetCategory category);
        Task UpdateAsync(BudgetCategory category);
        Task DeleteAsync(int id);

        Task<BudgetCategory> GetByNameAsync(string name);
    }
}
