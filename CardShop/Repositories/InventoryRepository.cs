using CardShop.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;
using CardShop.Repositories.Models;
using System.Data;


namespace CardShop.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public InventoryRepository(IConfiguration configuration, ILogger<InventoryRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            SqlMapper.AddTypeHandler(new CardSetCodeTypeHandler());

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

            try
            {

                // Perform the upsert operation (insert or replace)
                foreach (var item in inventoryItems)
                {
                    dbConnection.Execute(@"
                INSERT OR REPLACE INTO Inventory (ProductCode, SetCode, UserId) 
                VALUES (@ProductCode, @SetCode, @UserId)", new { ProductCode = item.ProductCode, SetCode = item.SetCode.ToString(), UserId = item.UserId });
                }

                // Update the Count column for the upserted items if needed
                foreach (var item in inventoryItems)
                {
                    dbConnection.Execute(@"
                UPDATE Inventory 
                SET Count = @Count 
                WHERE ProductCode = @ProductCode 
                AND SetCode = @SetCode 
                AND UserId = @UserId", new { ProductCode = item.ProductCode, SetCode = item.SetCode.ToString(), UserId = item.UserId, Count = item.Count });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error occurred while trying to insert or update inventory.", inventoryItems.ToArray());
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

    // Define a custom type handler for Enums.CardSetCode
    class CardSetCodeTypeHandler : SqlMapper.TypeHandler<Enums.CardSetCode>
    {
        public override Enums.CardSetCode Parse(object value)
        {
            // Convert the string representation to the enum value
            return Enum.Parse<Enums.CardSetCode>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, Enums.CardSetCode value)
        {
            // Set the parameter value as string
            parameter.Value = value.ToString();
        }
    }
}
