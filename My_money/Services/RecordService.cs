using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class RecordService : IRecordService
    {
        private readonly IRecordRepository _recordRepository;

        public RecordService(IRecordRepository recordRepository)
        {
            _recordRepository = recordRepository;
        }

        //TODO: Add validation and error handling as needed

        public async Task<List<Record>> GetAllRecordsAsync()
        {
            return await _recordRepository.GetAllAsync();
        }

        public async Task<Record?> GetRecordByIdAsync(int id)
        {
            return await _recordRepository.GetByIdAsync(id);
        }

        public async Task<int> AddRecordAsync(Record record)
        {
            return await _recordRepository.AddAsync(record);
        }

        public async Task UpdateRecordAsync(Record record)
        {
            await _recordRepository.UpdateAsync(record);
        }

        public async Task DeleteRecordAsync(int id)
        {
            await _recordRepository.DeleteAsync(id);
        }

        public async Task<List<Record>> GetRecordsByCategoryIdAsync(int categoryId)
        {
            return await _recordRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<List<Record>> GetRecordsByPeriodAsync(DateTime from, DateTime to)
        {
            return await _recordRepository.GetByPeriodAsync(from, to);
        }
    }
}
