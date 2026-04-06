using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class SavingsGoalRepository : ISavingsGoalRepository
    {
        private readonly string _connectionString;

        public SavingsGoalRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Get list for household member type Child in personal finances
        public async Task<List<SavingsGoal>> GetAllByHouseholdAndCreatedByAsync(int householdId, int createdByUserId)
        {
            var goals = new List<SavingsGoal>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM SavingsGoals WHERE HouseholdId = @householdId AND CreatedByUserId = @createdByUserId", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
                command.Parameters.AddWithValue("@createdByUserId", createdByUserId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        goals.Add(ReadGoal(reader));
                    }
                }
            }
            return goals;
        }

        // Get list for Household in shared finances
        public async Task<List<SavingsGoal>> GetAllByHouseholdIdAsync(int householdId)
        {
            var goals = new List<SavingsGoal>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM SavingsGoals WHERE HouseholdId = @householdId AND Scope = 'Shared'", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        goals.Add(ReadGoal(reader));
                    }
                }
            }
            return goals;
        }

        // Get list in personal finances
        public async Task<List<SavingsGoal>> GetAllByOwnerAsync(int ownerUserId)
        {
            var goals = new List<SavingsGoal>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM SavingsGoals WHERE OwnerUserId = @ownerUserId AND Scope = 'Personal'", connection);
                command.Parameters.AddWithValue("@ownerUserId", ownerUserId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        goals.Add(ReadGoal(reader));
                    }
                }
            }
            return goals;
        }

        public async Task<SavingsGoal?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM SavingsGoals WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadGoal(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(SavingsGoal goal)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO SavingsGoals (GoalName, Have, Need, HouseholdId, OwnerUserId, CreatedByUserId, Scope) " +
                    "VALUES (@name, @have, @need, @householdId, @ownerUserId, @createdByUserId, @scope); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@name", goal.GoalName);
                command.Parameters.AddWithValue("@have", goal.Have);
                command.Parameters.AddWithValue("@need", goal.Need);
                command.Parameters.AddWithValue("@householdId", goal.HouseholdId);
                command.Parameters.AddWithValue("@ownerUserId", goal.OwnerUserId);
                command.Parameters.AddWithValue("@createdByUserId", goal.CreatedByUserId);
                command.Parameters.AddWithValue("@scope", goal.Scope);
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(SavingsGoal goal)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE SavingsGoals SET GoalName = @name, Have = @have, Need = @need, HouseholdId = @householdId, OwnerUserId = @ownerUserId, " +
                    "CreatedByUserId = @createdByUserId, Scope = @scope WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@name", goal.GoalName);
                command.Parameters.AddWithValue("@have", goal.Have);
                command.Parameters.AddWithValue("@need", goal.Need);
                command.Parameters.AddWithValue("@householdId", goal.HouseholdId);
                command.Parameters.AddWithValue("@ownerUserId", goal.OwnerUserId);
                command.Parameters.AddWithValue("@createdByUserId", goal.CreatedByUserId);
                command.Parameters.AddWithValue("@scope", goal.Scope);
                command.Parameters.AddWithValue("@id", goal.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM SavingsGoals WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        private SavingsGoal ReadGoal(DbDataReader reader)
        {
            return new SavingsGoal
            {
                Id = Convert.ToInt32(reader["ID"]),
                GoalName = reader["GoalName"].ToString()!,
                Have = Convert.ToDecimal(reader["Have"]),
                Need = Convert.ToDecimal(reader["Need"]),
                HouseholdId = Convert.ToInt32(reader["HouseholdId"]),
                OwnerUserId = reader["OwnerUserId"] == DBNull.Value ? null : Convert.ToInt32(reader["OwnerUserId"]),
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"]),
                Scope = reader["Scope"].ToString()!
            };
        }
    }
}
