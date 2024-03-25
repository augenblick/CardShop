using CardShop.Interfaces;
using CardShop.Repositories;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace CardShop.Logic
{
    public class DbBuilder
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;

        public DbBuilder(IConfiguration configuration, ILogger<InventoryRepository> logger, IUserManager userManager)
        {
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task InitializeDb()
        {
            await CreateTables();
            await CreateDefaultUsers();
        }

        private async Task CreateTables()
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var inventory = await dbConnection.ExecuteAsync($@"
                        CREATE TABLE IF NOT EXISTS 
                        Inventory (
                            InventoryId INTEGER PRIMARY KEY AUTOINCREMENT
                                                    NOT NULL,
                                ProductCode TEXT    NOT NULL,
                                SetCode     TEXT    NOT NULL,
                                Count       INTEGER,
                                UserId      INTEGER NOT NULL
                                                    REFERENCES User (UserId),
                                UNIQUE (
                                    ProductCode,
                                    SetCode,
                                    UserId
                                )
                        )
                        STRICT;
            ");

            var user = await dbConnection.ExecuteAsync($@"
                        CREATE TABLE IF NOT EXISTS 
                            User (
                                UserId   INTEGER PRIMARY KEY AUTOINCREMENT
                                                 UNIQUE
                                                 NOT NULL,
                                Username TEXT    UNIQUE
                                                 NOT NULL,
                                Balance  TEXT    NOT NULL
                                                 DEFAULT (0)
                               
                        )
                        STRICT;
                        ");
        }

        private async Task CreateDefaultUsers()
        {
            var shopkeeperUsername = _configuration.GetValue<string>("ShopkeeperUsername") ?? "ShopKeeper";

            var defaultUserList = new List<User>
            {
                new User
                {
                    UserId = 0,
                    Username = shopkeeperUsername,
                    Balance = 0M
                },
                new User
                {
                    UserId = 1,
                    Username = "wormie",
                    Balance = 2500M
                }
            };
            
            foreach(var defaultUser in defaultUserList)
            {
                var existingUser = await _userManager.GetUser(defaultUser.Username);

                if (!string.IsNullOrWhiteSpace(existingUser.Username)) { continue; }

                var inserted = await _userManager.AddUser(defaultUser.Username, defaultUser.Balance);
            }
        }
    }
}
