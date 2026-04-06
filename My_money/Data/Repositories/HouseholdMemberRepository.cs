using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class HouseholdMemberRepository : IHouseholdMemberRepository
    {
        private readonly string _connectionString;

        public HouseholdMemberRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddAsync(HouseholdMember member)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO HouseholdMembers (HouseholdId, UserId, Role, CanManageBudget, CanManageMembers) " +
                    "VALUES (@householdId, @userId, @role, @canManageBudget, @canManageMembers); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@householdId", member.HouseholdId);
                command.Parameters.AddWithValue("@userId", member.UserId);
                command.Parameters.AddWithValue("@role", member.Role);
                command.Parameters.AddWithValue("@canManageBudget", member.CanManageBudget);
                command.Parameters.AddWithValue("@canManageMembers", member.CanManageMembers);
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM HouseholdMembers WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<HouseholdMember>> GetAllByHouseholdIdAsync(int householdId)
        {
            var members = new List<HouseholdMember>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM HouseholdMembers WHERE HouseholdId = @householdId", connection);
                command.Parameters.AddWithValue("@householdId", householdId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        members.Add(ReadHouseholdMember(reader));
                    }
                }
                return members;
            }
        }

        public async Task<HouseholdMember?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM HouseholdMembers WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadHouseholdMember(reader);
                    }
                }
                return null;
            }
        }

        public async Task<HouseholdMember?> GetByUserIdAsync(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM HouseholdMembers WHERE UserId = @userId", connection);
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return ReadHouseholdMember(reader);
                    }
                }
                return null;
            }
        }

        public async Task UpdateAsync(HouseholdMember member)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE HouseholdMembers SET HouseholdId = @householdId, UserId = @userId, Role = @role, " +
                    "CanManageBudget = @canManageBudget, CanManageMembers = @canManageMembers " +
                    "WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@householdId", member.HouseholdId);
                command.Parameters.AddWithValue("@userId", member.UserId);
                command.Parameters.AddWithValue("@role", member.Role);
                command.Parameters.AddWithValue("@canManageBudget", member.CanManageBudget);
                command.Parameters.AddWithValue("@canManageMembers", member.CanManageMembers);
                command.Parameters.AddWithValue("@id", member.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        private HouseholdMember ReadHouseholdMember(DbDataReader reader)
        {
            return new HouseholdMember
            {
                Id = Convert.ToInt32(reader["ID"]),
                HouseholdId = Convert.ToInt32(reader["HouseholdId"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                Role = reader["Role"].ToString()!,
                CanManageBudget = Convert.ToInt32(reader["CanManageBudget"]),
                CanManageMembers = Convert.ToInt32(reader["CanManageMembers"])
            };
        }
    }
}
