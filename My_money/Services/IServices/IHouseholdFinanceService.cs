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
        Task UpdateHouseholdFinanceAsync(HouseholdFinance finance);
        Task DeleteHouseholdFinanceAsync(int id);
    }
}
