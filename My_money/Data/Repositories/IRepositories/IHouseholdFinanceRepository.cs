using My_money.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IHouseholdFinanceRepository
    {
        Task<List<HouseholdFinance>> GetAllAsync();
        Task<HouseholdFinance?> GetByIdAsync(int id);
        Task<int> AddAsync(HouseholdFinance category);
        Task UpdateAsync(HouseholdFinance category);
        Task DeleteAsync(int id);
    }
}
