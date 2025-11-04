using My_money.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IRecordService
    {
        Task<List<Record>> GetAllRecordsAsync();
        Task<Record?> GetRecordByIdAsync(int id);
        Task<int> AddRecordAsync(Record record);
        Task UpdateRecordAsync(Record record);
        Task DeleteRecordAsync(int id);

        Task<List<Record>> GetRecordsByCategoryIdAsync(int categoryId);
        Task<List<Record>> GetRecordsByPeriodAsync(DateTime from, DateTime to);
    }
}
