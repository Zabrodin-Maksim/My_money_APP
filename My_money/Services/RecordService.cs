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
        private readonly IUserFinanceService _userFinanceService;

        public RecordService(IRecordRepository recordRepository, IUserFinanceService userFinanceService)
        {
            _recordRepository = recordRepository;
            _userFinanceService = userFinanceService;
        }

        // TODO: Add validation and error handling as needed

        public async Task<List<Record>> GetAllRecordsAsync()
        {
            return await _recordRepository.GetAllAsync();
        }

        public async Task<Record?> GetRecordByIdAsync(int id)
        {
            return await _recordRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Apply the expense to the user's balance before adding the record to ensure data consistency.
        /// </summary>
        public async Task<int> AddRecordAsync(Record record)
        {
            await _userFinanceService.ApplyExpenseAsync(record.Cost);
            return await _recordRepository.AddAsync(record);
        }

        public async Task UpdateRecordAsync(Record record)
        {
            await _recordRepository.UpdateAsync(record);
        }

        /// <summary>
        /// Deletes a financial record and restores its cost back to the user's balance.
        /// </summary>
        /// <param name="record">The record to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteRecordAsync(Record record)
        {
            await _recordRepository.DeleteAsync(record.Id);
            await _userFinanceService.AddToBalanceAsync(record.Cost);
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
