using My_money.Enums;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IRecordService
    {
        /// <summary>
        /// Get list for household member type Child in personal finances
        /// </summary>
        /// <param name="householdId"></param>
        /// <returns></returns>
        Task<List<Record>> GetAllByHouseholdAndCreatedByAsync(int householdId);
        /// <summary>
        /// Get list for Household in shared finances
        /// </summary>
        /// <param name="householdId"></param>
        /// <returns></returns>
        Task<List<Record>> GetAllByHouseholdIdAsync(int householdId);

        /// <summary>
        /// Get list in personal finances
        /// </summary>
        /// <returns></returns>
        Task<List<Record>> GetAllByOwnerAsync();

        /// <summary>
        /// Adds a new financial record and applies its effect to the corresponding finance context
        /// (user or household) based on its scope and type.
        /// </summary>
        /// <param name="record">The record to be added.</param>
        /// <param name="category">The category associated with the record.</param>
        /// <returns>The identifier of the newly created record.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the record amount is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the record scope does not match the category scope,
        /// or when the record type is invalid,
        /// or when an income record has an invalid or missing income target.
        /// </exception>
        Task<int> AddRecordAsync(Record record, BudgetCategory category);

        /// <summary>
        /// Deletes an existing financial record and reverses its effect on the corresponding finance context
        /// (user or household) based on its scope and type.
        /// </summary>
        /// <param name="record">The record to be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the record scope or type is invalid,
        /// or when an income record has an invalid or missing income target.
        /// </exception>
        Task DeleteRecordAsync(Record record);

        Task<List<Record>> GetRecordsByCategoryIdAsync(int categoryId);
        Task<List<Record>> GetRecordsByPeriodAsync(DateTime from, DateTime to, int? householdId);
    }
}
