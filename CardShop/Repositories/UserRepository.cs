using CardShop.Interfaces;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Data.Sqlite;

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

            var user = await dbConnection.QuerySingleAsync<User>($@"
                        SELECT *
                        FROM User
                        WHERE UserId = @UserId",
                        new
                        {
                            UserId = userId
                        });

            return user;
        }

        public async Task<User> GetUser(string username)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var user = await dbConnection.QueryAsync<User>($@"
                        SELECT *
                        FROM User
                        WHERE Username = @Username",
                        new
                        {
                            Username = username
                        });

            return user.FirstOrDefault();
        }

        public async Task<User> AddUser(string username, decimal balance) 
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var newUserId = await dbConnection.ExecuteScalarAsync<int>($@"
                        INSERT INTO User(Username, Balance) 
                        VALUES(@Username, @Balance)
                        RETURNING *;",
                        new
                        {
                            Username = username,
                            Balance = balance
                        });

            if (newUserId < 1)
            {
                _logger.LogError($"New User insert failed for username '{username}'");
                return new User();
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
    }
}
