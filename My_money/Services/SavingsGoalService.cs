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
        #region Dependency Injection Services
        private readonly ISavingsGoalRepository _savingsGoalRepository;
        private readonly IUserSessionService _userSessionService;
        #endregion

        public SavingsGoalService(ISavingsGoalRepository savingsGoalRepository, IUserSessionService userSessionService)
        {
            #region Dependency Injection
            _savingsGoalRepository = savingsGoalRepository;
            _userSessionService = userSessionService;
            #endregion
        }

        #region Helper Methods
        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            return _userSessionService.CurrentUser!.Id;
        }
        #endregion

        #region Query Methods
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
        #endregion

        #region Command Methods
        public async Task<int> AddSavingsGoal(SavingsGoal goal)
        {
            if (!_userSessionService.IsAuthenticated) throw new InvalidOperationException("User is not authenticated.");

            if (_userSessionService.CurrentHouseholdMember!.Role == nameof(HouseholdMemberRole.Child) && goal.Scope == RecordConstants.Scopes.Personal)
                throw new InvalidOperationException("Children cannot create personal goal.");

            return await _savingsGoalRepository.AddAsync(goal);
        }

        public async Task UpdateSavingsGoal(SavingsGoal goal)
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            if (_userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child))
            {
                var originalGoal = await _savingsGoalRepository.GetByIdAsync(goal.Id)
                    ?? throw new InvalidOperationException("Savings goal was not found.");

                if (originalGoal.Scope != RecordConstants.Scopes.Shared
                    || originalGoal.HouseholdId != _userSessionService.CurrentHouseholdMember.HouseholdId)
                {
                    throw new InvalidOperationException("Child accounts can only contribute to household goals.");
                }

                bool ownsGoal = originalGoal.CreatedByUserId == GetAuthenticatedUserId();

                if (!ownsGoal)
                {
                    bool changedProtectedFields =
                        goal.GoalName != originalGoal.GoalName
                        || goal.Need != originalGoal.Need
                        || goal.Scope != originalGoal.Scope
                        || goal.OwnerUserId != originalGoal.OwnerUserId
                        || goal.CreatedByUserId != originalGoal.CreatedByUserId
                        || goal.HouseholdId != originalGoal.HouseholdId;

                    if (changedProtectedFields)
                    {
                        throw new InvalidOperationException("Child accounts can fully edit only the household goals they created. For other family goals they can update only the current amount.");
                    }
                }

                goal.Scope = originalGoal.Scope;
                goal.OwnerUserId = originalGoal.OwnerUserId;
                goal.CreatedByUserId = originalGoal.CreatedByUserId;
                goal.HouseholdId = originalGoal.HouseholdId;
            }

            await _savingsGoalRepository.UpdateAsync(goal);
        }

        public async Task DeleteSavingsGoal(int id)
        {
            if (_userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child))
                throw new InvalidOperationException("Child accounts cannot delete savings goals.");

            await _savingsGoalRepository.DeleteAsync(id);
        }
        #endregion
    }
}
