using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class HouseholdFinanceRepository : IHouseholdFinanceRepository
    {
        private readonly string _connectionString;

        public HouseholdFinanceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddAsync(HouseholdFinance category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO HouseholdFinances (HouseholdId, Savings, Balance) " +
                    "VALUES (@householdId, @savings, @balance); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@householdId", category.HouseholdId);
                command.Parameters.AddWithValue("@savings", category.Savings);
                command.Parameters.AddWithValue("@balance", category.Balance);
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM HouseholdFinances WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<HouseholdFinance>> GetAllAsync()
        {
            var finances = new List<HouseholdFinance>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM HouseholdFinances", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        finances.Add(ReadHouseholdFinance(reader));
                    }
                }
            }
            return finances;
        }

        public async Task<HouseholdFinance?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM HouseholdFinances WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadHouseholdFinance(reader);
                }
            }
            return null;
        }

        public async Task UpdateAsync(HouseholdFinance category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE HouseholdFinances SET HouseholdId = @householdId, Savings = @savings, Balance = @balance WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", category.Id);
                command.Parameters.AddWithValue("@householdId", category.HouseholdId);
                command.Parameters.AddWithValue("@savings", category.Savings);
                command.Parameters.AddWithValue("@balance", category.Balance);
                await command.ExecuteNonQueryAsync();
            }
        }

        private HouseholdFinance ReadHouseholdFinance(DbDataReader reader)
        {
            return new HouseholdFinance
            {
                Id = Convert.ToInt32(reader["ID"]),
                HouseholdId = Convert.ToInt32(reader["HouseholdId"]),
                Savings = Convert.ToDecimal(reader["Savings"]),
                Balance = Convert.ToDecimal(reader["Balance"])
            };
        }
    }
}
