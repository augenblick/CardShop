using CardShop.Interfaces;
using CardShop.Repositories;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace CardShop.Logic
{
    public class DbBuilder
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;
        private readonly IShopManager _shopManager;

        public DbBuilder(IConfiguration configuration, ILogger<InventoryRepository> logger, IUserManager userManager, IShopManager shopManager)
        {
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _shopManager = shopManager;
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
                                                 DEFAULT (0),
                                Password ANY     NOT NULL
                                                 DEFAULT password123,
                                Email    ANY,
                                Role     ANY     NOT NULL
                                                 DEFAULT (0)
                        )
                        STRICT;
                        ");
        }

        private async Task CreateDefaultUsers()
        {
            var shopkeeperUsername = _configuration.GetValue<string>("ShopkeeperUsername") ?? "ShopKeeper";

            var defaultUserList = new List<SecureUser>
            {
                new SecureUser
                {
                    UserId = 1,
                    Username = shopkeeperUsername,
                    Balance = 0M,
                    Password = "password123",
                    Role = Enums.Role.Shop,
                    Email = "email@email.com"
                },
                new SecureUser
                {
                    UserId = 2,
                    Username = "wormie",
                    Balance = 25M,
                    Password = "password123",
                    Role = Enums.Role.Admin,
                    Email = "email@email.com"
                }
            };
            
            foreach(var defaultUser in defaultUserList)
            {
                var existingUser = await _userManager.GetUser(defaultUser.Username);

                if (!string.IsNullOrWhiteSpace(existingUser.Username)) { continue; }

                var inserted = await _userManager.AddUser(defaultUser.Username, defaultUser.Password, defaultUser.Email, defaultUser.Balance, defaultUser.Role, defaultUser.UserId);
            }
        }
    }
}
