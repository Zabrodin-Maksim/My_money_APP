using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories.IRepositories
{
    public interface IRecordRepository
    {
        string ConnectionString { get; }

        // Get for Child in personal finances
        Task<List<Record>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId);
        // Get for Household in shared finances
        Task<List<Record>> GetAllByHouseholdIdAsync(int householdId);
        // Get in personal finances
        Task<List<Record>> GetAllByOwnerAsync(int ownerUserId);
        Task<Record?> GetByIdAsync(int id);
        Task<int> AddAsync(Record record);
        Task<int> AddAsync(Record record, SQLiteConnection connection, SQLiteTransaction transaction);
        Task UpdateAsync(Record record);
        Task DeleteAsync(int id);
        Task DeleteAsync(int id, SQLiteConnection connection, SQLiteTransaction transaction);

        Task<List<Record>> GetByCategoryIdAsync(int categoryId);

        Task<List<Record>> GetHouseholdByPeriodAsync(DateTime from, DateTime to, int householdId);
        Task<List<Record>> GetPersonalByPeriodAsync(DateTime from, DateTime to, int ownerUserId);
        Task<List<Record>> GetChildByPeriodAsync(DateTime from, DateTime to, int householdId, int createdByUserId);
    }
}
