using My_money.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public interface ISavingsGoalRepository
    {
        Task<List<SavingsGoal>> GetAllAsync();
        Task<SavingsGoal> GetByIdAsync(int id);
        Task AddAsync(SavingsGoal goal);
        Task UpdateAsync(SavingsGoal goal);
        Task DeleteAsync(int id);
    }
}
