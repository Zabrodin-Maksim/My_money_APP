using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class HouseholdFinanceService : IHouseholdFinanceService
    {
        private readonly IHouseholdFinanceRepository _householdFinanceRepository;
        private readonly IHouseholdService _householdService;

        public HouseholdFinanceService(IHouseholdFinanceRepository householdFinanceRepository, IHouseholdService householdService)
        {
            _householdFinanceRepository = householdFinanceRepository;
            _householdService = householdService;
        }

        private async Task<HouseholdFinance> GetActualHouseholdFinanceAsync()
        {
            var household = await _householdService.GetHouseholdByAuthenticatedUserAsync() ?? throw new InvalidOperationException("Authenticated user does not belong to a household.");
            return await _householdFinanceRepository.GetByHouseholdIdAsync(household.Id) ?? throw new InvalidOperationException("Household finance not found.");
        }

        public async Task<int> AddHouseholdFinanceAsync(HouseholdFinance finance)
        {
            return await _householdFinanceRepository.AddAsync(finance);
        }

        public async Task ApplyExpenseAsync(decimal amount, IncomeTarget target)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            var actualHouseholdFinance = await GetActualHouseholdFinanceAsync();

            switch (target)
            {
                case IncomeTarget.Balance:
                    await UpdateHouseholdFinanceAsync(null, actualHouseholdFinance.Balance - amount);
                    break;
                case IncomeTarget.Savings:
                    await UpdateHouseholdFinanceAsync(actualHouseholdFinance.Savings - amount, null);
                    break;
                default:
                    throw new ArgumentException("Invalid income target", nameof(target));
            }
        }

        public async Task ApplyIncomeAsync(decimal amount, IncomeTarget target)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            var actualHouseholdFinance = await GetActualHouseholdFinanceAsync();

            switch (target)
            {
                case IncomeTarget.Balance:
                    await UpdateHouseholdFinanceAsync(null, actualHouseholdFinance.Balance + amount);
                    break;
                case IncomeTarget.Savings:
                    await UpdateHouseholdFinanceAsync(actualHouseholdFinance.Savings + amount, null);
                    break;
                default:
                    throw new ArgumentException("Invalid income target", nameof(target));
            }
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

        public async Task UpdateHouseholdFinanceAsync(decimal? savings, decimal? balance)
        {
            if (savings is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(savings), "Savings cannot be negative");
            }

            var actualHouseholdFinance = await GetActualHouseholdFinanceAsync();

            await _householdFinanceRepository.UpdateAsync(new HouseholdFinance 
            { 
                Id = actualHouseholdFinance.Id,
                HouseholdId = actualHouseholdFinance.HouseholdId,
                Savings = savings ?? actualHouseholdFinance.Savings,
                Balance = balance ?? actualHouseholdFinance.Balance
            });
        }
    }
}
