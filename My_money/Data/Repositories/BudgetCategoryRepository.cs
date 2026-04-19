using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class BudgetCategoryRepository : IBudgetCategoryRepository
    {
        private readonly string _connectionString;

        public BudgetCategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Get list for household member type Child in personal finances
        public async Task<List<BudgetCategory>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId)
        {
            var categories = new List<BudgetCategory>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories WHERE HouseholdId = @householdId AND CreatedByUserId = @createdByUserId", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
                command.Parameters.AddWithValue("@createdByUserId", createdByUserId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(ReadCategory(reader));
                    }
                }
            }
            return categories;
        }

        // Get list for Household in shared finances
        public async Task<List<BudgetCategory>> GetAllByHouseholdIdAsync(int householdId)
        {
            var categories = new List<BudgetCategory>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories WHERE HouseholdId = @householdId AND Scope = 'Shared'", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(ReadCategory(reader));
                    }
                }
            }
            return categories;
        }

        // Get list in personal finances
        public async Task<List<BudgetCategory>> GetAllByOwnerAsync(int ownerUserId)
        {
            var categories = new List<BudgetCategory>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories WHERE OwnerUserId = @ownerUserId AND Scope = 'Personal'", connection);
                command.Parameters.AddWithValue("@ownerUserId", ownerUserId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(ReadCategory(reader));
                    }
                }
            }
            return categories;
        }

        public async Task<BudgetCategory?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadCategory(reader);
                }
            }
            return null;
        }

        public async Task<BudgetCategory?> GetByNameAsync(string name, int householdId, int? ownerUserId, string scope)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories WHERE Name = @name AND HouseholdId = @householdId AND Scope = @scope " +
                    "AND ((@ownerUserId IS NULL AND OwnerUserId IS NULL) OR OwnerUserId = @ownerUserId)", connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@householdId", householdId);
                command.Parameters.AddWithValue("@ownerUserId", ownerUserId.HasValue ? ownerUserId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@scope", scope);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadCategory(reader);
                }
            }
            return null;
        }

        public async Task<int> AddAsync(BudgetCategory category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await AddAsync(category, connection, null);
            }
        }

        public async Task<int> AddAsync(BudgetCategory category, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            var command = new SQLiteCommand(
                "INSERT INTO BudgetCategories (Name, Plan, HouseholdId, OwnerUserId, Scope, CreatedByUserId) " +
                "VALUES (@name, @plan, @householdId, @ownerUserId, @scope, @createdByUserId); SELECT last_insert_rowid();", connection);

            if (transaction is not null)
            {
                command.Transaction = transaction;
            }

            command.Parameters.AddWithValue("@name", category.Name);
            command.Parameters.AddWithValue("@plan", category.Plan);
            command.Parameters.AddWithValue("@householdId", category.HouseholdId);
            command.Parameters.AddWithValue("@ownerUserId", category.OwnerUserId.HasValue ? category.OwnerUserId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@scope", category.Scope);
            command.Parameters.AddWithValue("@createdByUserId", category.CreatedByUserId);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task UpdateAsync(BudgetCategory category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE BudgetCategories SET Name = @name, Plan = @plan, HouseholdId = @householdId, OwnerUserId = @ownerUserId, " +
                    "Scope = @scope, CreatedByUserId = @createdByUserId WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@plan", category.Plan);
                command.Parameters.AddWithValue("@householdId", category.HouseholdId);
                command.Parameters.AddWithValue("@ownerUserId", category.OwnerUserId);
                command.Parameters.AddWithValue("@scope", category.Scope);
                command.Parameters.AddWithValue("@createdByUserId", category.CreatedByUserId);
                command.Parameters.AddWithValue("@id", category.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM BudgetCategories WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        private BudgetCategory ReadCategory(DbDataReader reader)
        {
            return new BudgetCategory
            {
                Id = Convert.ToInt32(reader["ID"]),
                Name = reader["Name"].ToString()!,
                Plan = Convert.ToDecimal(reader["Plan"]),
                HouseholdId = Convert.ToInt32(reader["HouseholdId"]),
                OwnerUserId = reader["OwnerUserId"] == DBNull.Value ? null : Convert.ToInt32(reader["OwnerUserId"]),
                Scope = reader["Scope"].ToString()!,
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"])
            };
        }
    }
}
