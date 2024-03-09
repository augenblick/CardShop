using CardShop.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;
using CardShop.Repositories.Models;


namespace CardShop.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IConfiguration _configuration;

        public InventoryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> InsertInventory(string productCode, string setCode, int count, string userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = await dbConnection.ExecuteScalarAsync<int>($@"
                        INSERT INTO Inventory(ProductCode, SetCode, Count, UserId) 
                        Values(@ProductCode, @SetCode, @Count, @UserId);",

                        new { 
                            ProductCode = productCode,
                            SetCode = setCode,
                            Count = count,
                            UserId = userId
                        });

            return updatedRowCount > 0;
        }
    }
}
