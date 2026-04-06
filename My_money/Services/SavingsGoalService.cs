using My_money.Constants;
using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class SavingsGoalService : ISavingsGoalService
    {
        private readonly ISavingsGoalRepository _savingsGoalRepository;
        private readonly IUserSessionService _userSessionService;

        public SavingsGoalService(ISavingsGoalRepository savingsGoalRepository, IUserSessionService userSessionService)
        {
            _savingsGoalRepository = savingsGoalRepository;
            _userSessionService = userSessionService;
        }

        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            return _userSessionService.CurrentUser!.Id;
        }

        public async Task<List<SavingsGoal>> GetAllByHouseholdAndCreatedByAsync(int householdId)
        {
            return await _savingsGoalRepository.GetAllByHouseholdAndCreatedByAsync(householdId, GetAuthenticatedUserId());
        }

        public async Task<List<SavingsGoal>> GetAllByHouseholdIdAsync(int householdId)
        {
            return await _savingsGoalRepository.GetAllByHouseholdIdAsync(householdId);
        }

        public async Task<List<SavingsGoal>> GetAllByOwnerAsync()
        {
            return await _savingsGoalRepository.GetAllByOwnerAsync(GetAuthenticatedUserId());
        }

        public async Task<SavingsGoal?> GetSavingsGoal(int id)
        {
            return await _savingsGoalRepository.GetByIdAsync(id);
        }

        public async Task<int> AddSavingsGoal(SavingsGoal goal)
        {
            if (!_userSessionService.IsAuthenticated) throw new InvalidOperationException("User is not authenticated.");

            if (_userSessionService.CurrentHouseholdMember!.Role == nameof(HouseholdMemberRole.Child) && goal.Scope == RecordConstants.Scopes.Personal)
                throw new InvalidOperationException("Children cannot create personal goal.");

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
