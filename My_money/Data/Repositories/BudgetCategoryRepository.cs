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

        public async Task<List<BudgetCategory>> GetAllAsync()
        {
            var categories = new List<BudgetCategory>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories", connection);
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

        public async Task<BudgetCategory> GetByIdAsync(int id)
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

        public async Task<BudgetCategory> GetByNameAsync(string name)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM BudgetCategories WHERE Name = @name", connection);
                command.Parameters.AddWithValue("@name", name);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadCategory(reader);
                }
            }
            return null;
        }

        public async Task AddAsync(BudgetCategory category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO BudgetCategories (Name, Plan, Spend) VALUES (@name, @plan, @spend)", connection);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@plan", category.Plan);
                command.Parameters.AddWithValue("@spend", category.Spend);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(BudgetCategory category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE BudgetCategories SET Name = @name, Plan = @plan, Spend = @spend WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@plan", category.Plan);
                command.Parameters.AddWithValue("@spend", category.Spend);
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
                Name = reader["Name"].ToString(),
                Plan = reader["Plan"] is DBNull ? 0 : Convert.ToSingle(reader["Plan"]),
                Spend = reader["Spend"] is DBNull ? 0 : Convert.ToSingle(reader["Spend"])
            };
        }
    }
}
