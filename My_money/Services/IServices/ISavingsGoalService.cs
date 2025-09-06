using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface ISavingsGoalService
    {
        Task<List<SavingsGoal>> GetAllSavingsGoals();
        Task<SavingsGoal?> GetSavingsGoal(int id);
        Task<int> AddSavingsGoal(SavingsGoal goal);
        Task UpdateSavingsGoal(SavingsGoal goal);
        Task DeleteSavingsGoal(int id);
    }
}
