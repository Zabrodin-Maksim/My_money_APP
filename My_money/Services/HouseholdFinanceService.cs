using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class HouseholdFinanceService : IHouseholdFinanceService
    {
        private readonly IHouseholdFinanceRepository _householdFinanceRepository;

        public HouseholdFinanceService(IHouseholdFinanceRepository householdFinanceRepository)
        {
            _householdFinanceRepository = householdFinanceRepository;
        }

        public async Task<int> AddHouseholdFinanceAsync(HouseholdFinance finance)
        {
            return await _householdFinanceRepository.AddAsync(finance);
        }

        public async Task DeleteHouseholdFinanceAsync(int id)
        {
            await _householdFinanceRepository.DeleteAsync(id);
        }

        public async Task<HouseholdFinance?> GetHouseholdFinanceByHouseholdIdAsync(int householdId)
        {
            return await _householdFinanceRepository.GetByHouseholdIdAsync(householdId);
        }

        public async Task<HouseholdFinance?> GetHouseholdFinanceByIdAsync(int id)
        {
            return await _householdFinanceRepository.GetByIdAsync(id);
        }

        public async Task UpdateHouseholdFinanceAsync(HouseholdFinance finance)
        {
            await _householdFinanceRepository.UpdateAsync(finance);
        }
    }
}
