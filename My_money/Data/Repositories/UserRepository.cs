using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddAsync(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO Users (Username, PasswordHash, DisplayName, IsActive) " +
                    "VALUES (@username, @passwordHash, @displayName, @isActive); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@displayName", user.DisplayName);
                command.Parameters.AddWithValue("@isActive", user.IsActive);
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
                    "DELETE FROM Users WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Users", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(ReadUser(reader));
                    }
                }
            }
            return users;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Users WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadUser(reader);
                }
            }
            return null;
        }

        public async Task<int> UpdateAsync(User user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE Users SET Username = @username, PasswordHash = @passwordHash, " +
                    "DisplayName = @displayName, IsActive = @isActive WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@displayName", user.DisplayName);
                command.Parameters.AddWithValue("@isActive", user.IsActive);
                command.Parameters.AddWithValue("@id", user.Id);
                return await command.ExecuteNonQueryAsync();
            }
        }

        private User ReadUser(DbDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["ID"]),
                Username = reader["Username"].ToString()!,
                PasswordHash = reader["PasswordHash"].ToString()!,
                DisplayName = reader["DisplayName"].ToString()!,
                IsActive = Convert.ToInt32(reader["IsActive"])
            };
        }
    }
}
