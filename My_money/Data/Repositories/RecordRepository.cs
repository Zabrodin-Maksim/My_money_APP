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

        // Get list for household member type Child in personal finances
        public async Task<List<Record>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId)
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Records WHERE HouseholdId = @householdId AND CreatedByUserId = @createdByUserId", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
                command.Parameters.AddWithValue("@createdByUserId", createdByUserId);
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

        // Get list for Household in shared finances
        public async Task<List<Record>> GetAllByHouseholdIdAsync(int householdId)
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Records WHERE HouseholdId = @householdId AND Scope = 'Shared'", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
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

        // Get list in personal finances
        public async Task<List<Record>> GetAllByOwnerAsync(int ownerUserId)
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Records WHERE OwnerUserId = @ownerUserId AND Scope = 'Personal'", connection);
                command.Parameters.AddWithValue("@ownerUserId", ownerUserId);
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
                    "INSERT INTO Records (Amount, CategoryId, DateTimeOccured, Description, HouseholdId, OwnerUserId, CreatedByUserId, Scope, Type, IncomeTarget) " +
                    "VALUES (@amount, @categoryId, @dateTimeOccured, @description, @householdId, @ownerUserId, @createdByUserId, @scope, @type, @incomeTarget); SELECT last_insert_rowid();",
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
                command.Parameters.AddWithValue("@incomeTarget", record.IncomeTarget);
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
                    "HouseholdId = @householdId, OwnerUserId = @ownerUserId, CreatedByUserId = @createdByUserId, Scope = @scope, Type = @type, IncomeTarget = @incomeTarget WHERE ID = @id",
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
                command.Parameters.AddWithValue("@incomeTarget", record.IncomeTarget);
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

        public async Task<List<Record>> GetByPeriodAsync(DateTime from, DateTime to, int? householdId, int? ownerUserId)
        {
            if (!householdId.HasValue && !ownerUserId.HasValue)
                throw new ArgumentException("Either householdId or ownerUserId must be provided.");

            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                SQLiteCommand command;
                if (householdId.HasValue)
                {
                    command = new SQLiteCommand(
                    "SELECT * FROM Records WHERE DateTimeOccured >= @from AND DateTimeOccured <= @to AND HouseholdId = @householdId", connection);
                    command.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@householdId", householdId.Value);
                }
                else
                {
                    command = new SQLiteCommand(
                    "SELECT * FROM Records WHERE DateTimeOccured >= @from AND DateTimeOccured <= @to AND OwnerUserId = @ownerUserId", connection);
                    command.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ownerUserId", ownerUserId.Value);
                }
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

        public async Task<List<Record>> GetHouseholdByPeriodAsync(DateTime from, DateTime to, int householdId)
        {
            return await GetByPeriodInternalAsync(
                "SELECT * FROM Records WHERE DateTimeOccured >= @from AND DateTimeOccured <= @to " +
                "AND HouseholdId = @householdId AND Scope = 'Shared'",
                from,
                to,
                ("@householdId", householdId));
        }

        public async Task<List<Record>> GetPersonalByPeriodAsync(DateTime from, DateTime to, int ownerUserId)
        {
            return await GetByPeriodInternalAsync(
                "SELECT * FROM Records WHERE DateTimeOccured >= @from AND DateTimeOccured <= @to " +
                "AND OwnerUserId = @ownerUserId AND Scope = 'Personal'",
                from,
                to,
                ("@ownerUserId", ownerUserId));
        }

        public async Task<List<Record>> GetChildByPeriodAsync(DateTime from, DateTime to, int householdId, int createdByUserId)
        {
            return await GetByPeriodInternalAsync(
                "SELECT * FROM Records WHERE DateTimeOccured >= @from AND DateTimeOccured <= @to " +
                "AND HouseholdId = @householdId AND CreatedByUserId = @createdByUserId AND Scope = 'Shared'",
                from,
                to,
                ("@householdId", householdId),
                ("@createdByUserId", createdByUserId));
        }

        private async Task<List<Record>> GetByPeriodInternalAsync(string query, DateTime from, DateTime to, params (string Name, object Value)[] parameters)
        {
            var records = new List<Record>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@from", from.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@to", to.ToString("yyyy-MM-dd HH:mm:ss"));

                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Name, parameter.Value);
                }

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
                DateTimeOccurred = reader["DateTimeOccured"] != DBNull.Value ? DateTime.Parse(reader["DateTimeOccured"].ToString()!) : null,
                Description = reader["Description"]?.ToString(),
                HouseholdId = Convert.ToInt32(reader["HouseholdId"]),
                OwnerUserId = reader["OwnerUserId"] == DBNull.Value ? null : Convert.ToInt32(reader["OwnerUserId"]),
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"]),
                Scope = reader["Scope"]!.ToString()!,
                Type = reader["Type"]!.ToString()!,
                IncomeTarget = reader["IncomeTarget"]?.ToString()
            };
        }
    }
}
