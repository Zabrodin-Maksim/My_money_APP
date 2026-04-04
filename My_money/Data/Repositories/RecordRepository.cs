using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class RecordRepository : IRecordRepository
    {
        private readonly string _connectionString;

        public RecordRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Record>> GetAllAsync()
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Records", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        records.Add(ReadRecord(reader));
                    }
                }
            }
            return records;
        }

        public async Task<Record?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Records WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadRecord(reader);
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Record record)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO Records (Amount, CategoryId, DateTimeOccured, Description, HouseholdId, OwnerUserId, CreatedByUserId, Scope, Type) " +
                    "VALUES (@amount, @categoryId, @dateTimeOccured, @description, @householdId, @ownerUserId, @createdByUserId, @scope, @type); SELECT last_insert_rowid();",
                    connection);
                command.Parameters.AddWithValue("@amount", record.Amount);
                command.Parameters.AddWithValue("@categoryId", record.CategoryId);
                command.Parameters.AddWithValue("@dateTimeOccured", record.DateTimeOccurred?.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@description", record.Description ?? "");
                command.Parameters.AddWithValue("@householdId", record.HouseholdId);
                command.Parameters.AddWithValue("@ownerUserId", record.OwnerUserId);
                command.Parameters.AddWithValue("@createdByUserId", record.CreatedByUserId);
                command.Parameters.AddWithValue("@scope", record.Scope);
                command.Parameters.AddWithValue("@type", record.Type);
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Record record)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE Records SET Amount = @amount, CategoryId = @categoryId, DateTimeOccured = @dateTimeOccured, Description = @description, " +
                    "HouseholdId = @householdId, OwnerUserId = @ownerUserId, CreatedByUserId = @createdByUserId, Scope = @scope, Type = @type WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@amount", record.Amount);
                command.Parameters.AddWithValue("@categoryId", record.CategoryId);
                command.Parameters.AddWithValue("@dateTimeOccured", record.DateTimeOccurred?.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@description", record.Description ?? "");
                command.Parameters.AddWithValue("@householdId", record.HouseholdId);
                command.Parameters.AddWithValue("@ownerUserId", record.OwnerUserId);
                command.Parameters.AddWithValue("@createdByUserId", record.CreatedByUserId);
                command.Parameters.AddWithValue("@scope", record.Scope);
                command.Parameters.AddWithValue("@type", record.Type);
                command.Parameters.AddWithValue("@id", record.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("DELETE FROM Records WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Record>> GetByCategoryIdAsync(int categoryId)
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Records WHERE CategoryId = @categoryId", connection);
                command.Parameters.AddWithValue("@categoryId", categoryId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        records.Add(ReadRecord(reader));
                    }
                }
            }
            return records;
        }

        public async Task<List<Record>> GetByPeriodAsync(DateTime from, DateTime to)
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "SELECT * FROM Records WHERE DateTimeOccured >= @from AND DateTimeOccured <= @to", connection);
                command.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd HH:mm:ss"));
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        records.Add(ReadRecord(reader));
                    }
                }
            }
            return records;
        }

        private Record ReadRecord(DbDataReader reader)
        {
            return new Record
            {
                Id = Convert.ToInt32(reader["ID"]),
                Amount = Convert.ToDecimal(reader["Amount"]),
                CategoryId = reader["CategoryId"] == DBNull.Value ? null : Convert.ToInt32(reader["CategoryId"]),
                DateTimeOccurred = reader["DateTimeOccured"] != DBNull.Value ? DateTime.Parse(reader["DateTimeOccured"].ToString()!) : (DateTime?)null,
                Description = reader["Description"]?.ToString(),
                HouseholdId = Convert.ToInt32(reader["HouseholdId"]),
                OwnerUserId = reader["OwnerUserId"] == DBNull.Value ? null : Convert.ToInt32(reader["OwnerUserId"]),
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"]),
                Scope = reader["Scope"]!.ToString()!,
                Type = reader["Type"]!.ToString()!
            };
        }
    }
}
