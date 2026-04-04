using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public class HouseholdRepository : IHouseholdRepository
    {
        private readonly string _connectionString;

        public HouseholdRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddAsync(Household household)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO Households (Name, CreatedByUserId) " +
                    "VALUES (@name, @createdByUserId); SELECT last_insert_rowid();", connection);
                command.Parameters.AddWithValue("@name", household.Name);
                command.Parameters.AddWithValue("@createdByUserId", household.CreatedByUserId);
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "DELETE FROM Households WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Household>> GetAllAsync()
        {
            var households = new List<Household>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Households", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        households.Add(ReadHousehold(reader));
                    }
                }
            }
            return households;
        }

        public async Task<Household?> GetByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand("SELECT * FROM Households WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return ReadHousehold(reader);
                }
            }
            return null;
        }

        public async Task UpdateAsync(Household household)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "UPDATE Households SET Name = @name, CreatedByUserId = @createdByUserId " +
                    "WHERE ID = @id", connection);
                command.Parameters.AddWithValue("@name", household.Name);
                command.Parameters.AddWithValue("@createdByUserId", household.CreatedByUserId);
                command.Parameters.AddWithValue("@id", household.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public Household ReadHousehold(DbDataReader reader)
        {
            return new Household
            {
                Id = Convert.ToInt32(reader["ID"]),
                Name = reader["Name"].ToString()!,
                CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"])
            };
        }
    }
}
