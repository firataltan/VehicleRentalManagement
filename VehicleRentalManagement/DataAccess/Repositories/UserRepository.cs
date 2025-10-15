using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using VehicleRentalManagement.Models;

namespace VehicleRentalManagement.DataAccess.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly DatabaseConnection _db;

        public UserRepository(DatabaseConnection db)
        {
            _db = db;
        }

        public User ValidateUser(string username, string password)
        {
            User user = null;
            var passwordHash = HashPassword(password);

            using (var conn = _db.GetConnection())
            {
                var query = @"SELECT * FROM Users 
                             WHERE Username = @Username 
                             AND PasswordHash = @PasswordHash 
                             AND IsActive = 1";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = MapToUser(reader);
                            UpdateLastLogin(user.UserId);
                        }
                    }
                }
            }

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();

            using (var conn = _db.GetConnection())
            {
                var query = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY FullName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(MapToUser(reader));
                        }
                    }
                }
            }

            return users;
        }

        public User GetById(int id)
        {
            User user = null;

            using (var conn = _db.GetConnection())
            {
                var query = "SELECT * FROM Users WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = MapToUser(reader);
                        }
                    }
                }
            }

            return user;
        }

        public int Add(User entity)
        {
            using (var conn = _db.GetConnection())
            {
                var query = @"INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, CreatedDate)
                             VALUES (@Username, @PasswordHash, @FullName, @Email, @Role, @CreatedDate);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", entity.Username);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(entity.PasswordHash));
                    cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                    cmd.Parameters.AddWithValue("@Email", entity.Email);
                    cmd.Parameters.AddWithValue("@Role", entity.Role);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public bool Update(User entity)
        {
            using (var conn = _db.GetConnection())
            {
                var query = @"UPDATE Users 
                             SET FullName = @FullName, 
                                 Email = @Email,
                                 Role = @Role
                             WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId);
                    cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                    cmd.Parameters.AddWithValue("@Email", entity.Email);
                    cmd.Parameters.AddWithValue("@Role", entity.Role);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int id)
        {
            using (var conn = _db.GetConnection())
            {
                var query = "UPDATE Users SET IsActive = 0 WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private void UpdateLastLogin(int userId)
        {
            using (var conn = _db.GetConnection())
            {
                var query = "UPDATE Users SET LastLoginDate = @LoginDate WHERE UserId = @UserId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@LoginDate", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private User MapToUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate"))
            };
        }
    }
}