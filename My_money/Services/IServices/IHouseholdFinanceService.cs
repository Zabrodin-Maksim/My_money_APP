using My_money.Enums;
using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IHouseholdFinanceService
    {
        Task<HouseholdFinance?> GetHouseholdFinanceByIdAsync(int id);
        Task<HouseholdFinance?> GetHouseholdFinanceByHouseholdIdAsync(int householdId);
        Task<int> AddHouseholdFinanceAsync(HouseholdFinance finance);
        Task UpdateHouseholdFinanceAsync(decimal? savings, decimal? balance);
        Task ApplyExpenseAsync(decimal amount, IncomeTarget target);
        Task ApplyIncomeAsync(decimal amount, IncomeTarget target);
        Task DeleteHouseholdFinanceAsync(int id);
    }
}
