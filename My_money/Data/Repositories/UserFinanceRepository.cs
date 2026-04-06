using My_money.Data.Repositories.IRepositories;
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

        public async Task<UserFinance?> GetByUserIdAsync(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await GetByUserIdAsync(userId, connection, null!);
            }
        }

        public async Task<UserFinance?> GetByUserIdAsync(int userId, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            var command = new SQLiteCommand("SELECT * FROM UserFinances WHERE UserId = @userId", connection);
            if (transaction is not null)
            {
                command.Transaction = transaction;
            }

            command.Parameters.AddWithValue("@userId", userId);
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return ReadUserFinance(reader);
                }
            }

            return null;
        }

        public async Task<int> AddAsync(UserFinance userFinance)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                return await AddAsync(userFinance, connection, null!);
            }
        }

        public async Task<int> AddAsync(UserFinance userFinance, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            var command = new SQLiteCommand(
                "INSERT INTO UserFinances (Savings, Balance, UserId) VALUES (@savings, @balance, @userId); SELECT last_insert_rowid();",
                connection);
            if (transaction is not null)
            {
                command.Transaction = transaction;
            }

            command.Parameters.AddWithValue("@savings", userFinance.Savings);
            command.Parameters.AddWithValue("@balance", userFinance.Balance);
            command.Parameters.AddWithValue("@userId", userFinance.UserId);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task UpdateAsync(UserFinance userFinance)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                await UpdateAsync(userFinance, connection, null!);
            }
        }

        public async Task UpdateAsync(UserFinance userFinance, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            var command = new SQLiteCommand(
                "UPDATE UserFinances SET Savings = @savings, Balance = @balance, UserId = @userId WHERE ID = @id",
                connection);
            if (transaction is not null)
            {
                command.Transaction = transaction;
            }

            command.Parameters.AddWithValue("@id", userFinance.Id);
            command.Parameters.AddWithValue("@savings", userFinance.Savings);
            command.Parameters.AddWithValue("@balance", userFinance.Balance);
            command.Parameters.AddWithValue("@userId", userFinance.UserId);
            await command.ExecuteNonQueryAsync();
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
                Savings = Convert.ToDecimal(reader["Savings"]),
                Balance = Convert.ToDecimal(reader["Balance"]),
                UserId = Convert.ToInt32(reader["UserId"])
            };
        }
    }
}
