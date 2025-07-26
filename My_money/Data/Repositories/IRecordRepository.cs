using My_money.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public interface IRecordRepository
    {
        Task<List<Record>> GetAllAsync();
        Task<Record> GetByIdAsync(int id);
        Task AddAsync(Record record);
        Task UpdateAsync(Record record);
        Task DeleteAsync(int id);

        Task<List<Record>> GetByCategoryIdAsync(int categoryId);

        Task<List<Record>> GetByPeriodAsync(DateTime from, DateTime to);
    }
}
