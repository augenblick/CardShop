using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace CardShop.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<User> GetUser(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var user = await dbConnection.QueryAsync<User>($@"
                        SELECT *
                        FROM User
                        WHERE UserId = @UserId",
                        new
                        {
                            UserId = userId
                        });

            return user.FirstOrDefault();
        }

        public async Task<User> GetUser(string username)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var user = await dbConnection.QueryAsync<User>($@"
                        SELECT *
                        FROM User
                        WHERE Username LIKE @Username",
                        new
                        {
                            Username = username
                        });

            return user.FirstOrDefault();
        }

        public async Task<SecureUser> GetSecureUser(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var user = await dbConnection.QueryAsync<SecureUser>($@"
                        SELECT *
                        FROM User
                        WHERE UserId = @UserId",
                        new
                        {
                            UserId = userId
                        });

            return user.FirstOrDefault();
        }

        public async Task<SecureUser> GetSecureUser(string username)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var user = await dbConnection.QueryAsync<SecureUser>($@"
                        SELECT *
                        FROM User
                        WHERE Username LIKE @Username",
                        new
                        {
                            Username = username
                        });

            return user.FirstOrDefault();
        }

        public async Task<User> AddUser(string username, string password, string email = null, decimal balance = 0m, Role role = Role.User) 
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var newUserId = await dbConnection.ExecuteScalarAsync<int>($@"
                        INSERT INTO User(Username, Balance, Email, Password, Role) 
                        VALUES(@Username, @Balance, @Email, @Password, @Role)
                        RETURNING *;",
                        new
                        {
                            Username = username,
                            Balance = balance,
                            Email = email,
                            Password = password,
                            Role = role
                        });

            if (newUserId < 1)
            {
                _logger.LogError($"New User insert failed for username '{username}'");
                return null;
            }

            var newUser = await dbConnection.QuerySingleAsync<User>($@"
                        SELECT *
                        FROM User
                        WHERE UserId = @UserId;",
                        new
                        {
                            UserId = newUserId
                        });

            return newUser;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = await dbConnection.ExecuteAsync($@"
                        DELETE 
                        FROM USER
                        WHERE UserId = @UserId;",
                        new
                        {
                            UserId = userId
                        });

            return updatedRowCount > 0;
        }

        public async Task<bool> SetUserBalance(int userId, decimal newBalance)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = await dbConnection.ExecuteAsync($@"
                        UPDATE User
                        SET Balance = @Balance
                        WHERE UserId = @UserId",
                        new
                        {
                            Balance = newBalance,
                            UserId = userId
                        });

            return updatedRowCount > 0;
        }

        public async Task<bool> SetUserRole(int userId, Role role)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = await dbConnection.ExecuteAsync($@"
                        UPDATE User
                        SET Role = @Role
                        WHERE UserId = @UserId",
                        new
                        {
                            Role = role,
                            UserId = userId
                        });

            return updatedRowCount > 0;
        }

        public async Task<bool> UpdateUserPassword(int userId, string newPassword)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = await dbConnection.ExecuteAsync($@"
                        UPDATE User
                        SET Password = @Password
                        WHERE UserId = @UserId",
                        new
                        {
                            Password = newPassword,
                            UserId = userId
                        });

            return updatedRowCount > 0;
        }
    }
}
