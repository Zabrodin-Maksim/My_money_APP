using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IHouseholdFinanceRepository
    {
        Task<HouseholdFinance?> GetByIdAsync(int id);
        Task<HouseholdFinance?> GetByHouseholdIdAsync(int householdId);
        Task<int> AddAsync(HouseholdFinance finance);
        Task UpdateAsync(HouseholdFinance finance);
        Task DeleteAsync(int id);
    }
}
