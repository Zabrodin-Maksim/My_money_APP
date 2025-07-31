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

        public async Task<List<SavingsGoal>> GetAllAsync()
        {
            var goals = new List<SavingsGoal>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM SavingsGoals", connection);
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

        public async Task<SavingsGoal> GetByIdAsync(int id)
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

        public async Task AddAsync(SavingsGoal goal)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO SavingsGoals (GoalName, Have, Need) VALUES (@name, @have, @need); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@name", goal.GoalName);
                command.Parameters.AddWithValue("@have", goal.Have);
                command.Parameters.AddWithValue("@need", goal.Need);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(SavingsGoal goal)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE SavingsGoals SET GoalName = @name, Have = @have, Need = @need WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@name", goal.GoalName);
                command.Parameters.AddWithValue("@have", goal.Have);
                command.Parameters.AddWithValue("@need", goal.Need);
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
                GoalName = reader["GoalName"].ToString(),
                Have = Convert.ToSingle(reader["Have"]),
                Need = Convert.ToSingle(reader["Need"])
            };
        }
    }
}
