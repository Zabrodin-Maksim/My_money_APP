using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class SavingsGoalService : ISavingsGoalService
    {
        private readonly ISavingsGoalRepository _savingsGoalRepository;

        public SavingsGoalService(ISavingsGoalRepository savingsGoalRepository)
        {
            _savingsGoalRepository = savingsGoalRepository;
        }

        public async Task<List<SavingsGoal>> GetAllSavingsGoals()
        {
            return await _savingsGoalRepository.GetAllAsync();
        }

        public async Task<SavingsGoal?> GetSavingsGoal(int id)
        {
            return await _savingsGoalRepository.GetByIdAsync(id);
        }

        public async Task<int> AddSavingsGoal(SavingsGoal goal)
        {
            return await _savingsGoalRepository.AddAsync(goal);
        }

        public async Task UpdateSavingsGoal(SavingsGoal goal)
        {
            await _savingsGoalRepository.UpdateAsync(goal);
        }

        public async Task DeleteSavingsGoal(int id)
        {
            await _savingsGoalRepository.DeleteAsync(id);
        }
    }
}
