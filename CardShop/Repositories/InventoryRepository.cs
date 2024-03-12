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

        public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var inventory = await dbConnection.QueryAsync<Inventory>($@"
                        SELECT *
                        FROM Inventory
                        WHERE UserId = @UserId",
                        new
                        {
                            UserId = userId
                        });

            return inventory.AsList();
        }

        public async Task<List<Inventory>> UpsertInventory(List<Inventory> inventoryItems)
        {
            var returnInventory = new List<Inventory>();

            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            dbConnection.Open();

            var transaction = dbConnection.BeginTransaction();

            // TODO: make setcode appear as string in DB
            // TODO: make setcode reflect actual set

            try
            {

                // Perform the upsert operation (insert or replace)
                foreach (var item in inventoryItems)
                {
                    dbConnection.Execute(@"
                INSERT OR REPLACE INTO Inventory (ProductCode, SetCode, UserId) 
                VALUES (@ProductCode, @SetCode, @UserId)", item);
                }

                // Update the Count column for the upserted items if needed
                foreach (var item in inventoryItems)
                {
                    dbConnection.Execute(@"
                UPDATE Inventory 
                SET Count = @Count 
                WHERE ProductCode = @ProductCode 
                AND SetCode = @SetCode 
                AND UserId = @UserId", item);
                }

            }
            catch (Exception ex)
            {
                // TODO: log error
                transaction.Rollback();
            }

            //TODO: fix the following:

            // Retrieve upserted items based on the criteria used for upsert operation
            //var upsertedItems = dbConnection.Query<Inventory>(@"
            //SELECT * FROM Inventory 
            //WHERE (ProductCode, SetCode, UserId) IN @Keys", new { Keys = inventoryItems.Select(x => (x.ProductCode, x.SetCode, x.UserId)) });

            transaction.Commit();

            return new List<Inventory>(); // upsertedItems.ToList();

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
