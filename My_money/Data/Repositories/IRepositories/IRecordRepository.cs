using My_money.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IRecordRepository
    {
        // Get for Child in personal finances
        Task<List<Record>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId);
        // Get for Household in shared finances
        Task<List<Record>> GetAllByHouseholdIdAsync(int householdId);
        // Get in personal finances
        Task<List<Record>> GetAllByOwnerAsync(int ownerUserId);
        Task<Record?> GetByIdAsync(int id);
        Task<int> AddAsync(Record record);
        Task UpdateAsync(Record record);
        Task DeleteAsync(int id);

        Task<List<Record>> GetByCategoryIdAsync(int categoryId);

        Task<List<Record>> GetByPeriodAsync(DateTime from, DateTime to, int? householdId, int? ownerUserId);
    }
}
