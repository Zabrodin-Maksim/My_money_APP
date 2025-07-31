using My_money.Model;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class UserFinanceRepository : IUserFinanceRepository
    {
        private readonly string _connectionString;

        public UserFinanceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<UserFinance> GetAsync()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM UserFinances LIMIT 1", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadUserFinance(reader);
                    }
                }
            }
            return null;
        }

        public async Task AddAsync(UserFinance userFinance)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO UserFinances (Savings, Balance) VALUES (@savings, @balance)", connection);
                command.Parameters.AddWithValue("@savings", userFinance.Savings);
                command.Parameters.AddWithValue("@balance", userFinance.Balance);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(UserFinance userFinance)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE UserFinances SET Savings = @savings, Balance = @balance WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@savings", userFinance.Savings);
                command.Parameters.AddWithValue("@balance", userFinance.Balance);
                command.Parameters.AddWithValue("@id", userFinance.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM UserFinances WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        private UserFinance ReadUserFinance(DbDataReader reader)
        {
            return new UserFinance
            {
                Id = reader["ID"] is DBNull ? 0 : Convert.ToInt32(reader["ID"]),
                Savings = reader["Savings"] is DBNull ? 0 : Convert.ToSingle(reader["Savings"]),
                Balance = reader["Balance"] is DBNull ? 0 : Convert.ToSingle(reader["Balance"])
            };
        }
    }
}
