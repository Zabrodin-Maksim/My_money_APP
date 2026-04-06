using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class HouseholdService : IHouseholdService
    {
        private readonly IHouseholdRepository _householdRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IHouseholdMemberRepository _householdMemberRepository;

        public HouseholdService(IHouseholdRepository householdRepository, IUserSessionService userSessionService, IHouseholdMemberRepository householdMemberRepository)
        {
            _householdRepository = householdRepository;
            _userSessionService = userSessionService;
            _householdMemberRepository = householdMemberRepository;
        }

        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            return _userSessionService.CurrentUser!.Id;
        }

        public async Task<int> AddHouseholdAsync(Household household)
        {
            return await _householdRepository.AddAsync(household);
        }

        public async Task DeleteHouseholdAsync(int id)
        {
            await _householdRepository.DeleteAsync(id);
        }

        public async Task<List<Household>> GetAllHouseholdsAsync()
        {
            return await _householdRepository.GetAllAsync();
        }

        public async Task<Household?> GetHouseholdByAuthenticatedUserAsync()
        {
            var householdMember = await _householdMemberRepository.GetByUserIdAsync(GetAuthenticatedUserId()) ?? throw new InvalidOperationException("Household member not found.");
            return await _householdRepository.GetByIdAsync(householdMember.HouseholdId);
        }

        public async Task<Household?> GetHouseholdByIdAsync(int id)
        {
            return await _householdRepository.GetByIdAsync(id);
        }

        public async Task UpdateHouseholdAsync(Household household)
        {
            await _householdRepository.UpdateAsync(household);
        }
    }
}
