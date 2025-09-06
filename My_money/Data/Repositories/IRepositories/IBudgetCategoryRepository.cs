using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IBudgetCategoryRepository
    {
        Task<List<BudgetCategory>> GetAllAsync();
        Task<BudgetCategory?> GetByIdAsync(int id);
        Task<int> AddAsync(BudgetCategory category);
        Task UpdateAsync(BudgetCategory category);
        Task DeleteAsync(int id);

        Task<BudgetCategory?> GetByNameAsync(string name);
    }
}
