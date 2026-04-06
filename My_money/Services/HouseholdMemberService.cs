using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace My_money.Services
{
    public class HouseholdMemberService : IHouseholdMemberService
    {
        private readonly IHouseholdMemberRepository _householdMemberRepository;
        private readonly IUserSessionService _userSessionService;

        public HouseholdMemberService(IHouseholdMemberRepository householdMemberRepository, IUserSessionService userSessionService)
        {
            _householdMemberRepository = householdMemberRepository;
            _userSessionService = userSessionService;
        }

        public async Task<int> AddHouseholdMemberAsync(HouseholdMember member)
        {
            return await _householdMemberRepository.AddAsync(member);
        }

        public async Task DeleteHouseholdMemberAsync(int id)
        {
            await _householdMemberRepository.DeleteAsync(id);
        }

        public async Task<List<HouseholdMember>> GetAllHouseholdMembersByHouseholdIdAsync()
        {
            if (_userSessionService.IsAuthenticated)
            {
                var member = _userSessionService.CurrentHouseholdMember;
                return await _householdMemberRepository.GetAllByHouseholdIdAsync(member!.HouseholdId);
            }
            throw new InvalidOperationException("User is not authenticated.");
        }

        public async Task<HouseholdMember?> GetHouseholdMemberByIdAsync(int id)
        {
            return await _householdMemberRepository.GetByIdAsync(id);
        }

        public async Task<HouseholdMember?> GetHouseholdMemberByAuthenticatedUserAsync()
        {
            if (_userSessionService.IsAuthenticated)
            {
                return await _householdMemberRepository.GetByUserIdAsync(_userSessionService.CurrentUser!.Id);
            }
            throw new InvalidOperationException("User is not authenticated.");
        }

        public async Task<HouseholdMember?> GetHouseholdMemberByUserIdAsync(int userId)
        {
            return await _householdMemberRepository.GetByUserIdAsync(userId);
        }

        public async Task UpdateHouseholdMemberAsync(HouseholdMember member)
        {
            await _householdMemberRepository.UpdateAsync(member);
        }
    }
}
