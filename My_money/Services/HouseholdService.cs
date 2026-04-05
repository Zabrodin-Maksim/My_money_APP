using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class HouseholdService : IHouseholdService
    {
        private readonly IHouseholdRepository _householdRepository;

        public HouseholdService(IHouseholdRepository householdRepository)
        {
            _householdRepository = householdRepository;
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
