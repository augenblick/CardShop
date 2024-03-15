using CardShop.Interfaces;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CardShop.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;

        public UserRepository(IConfiguration configuration)
        {
            _configuration = configuration;
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
