using CardShop.Interfaces;
using Microsoft.Data.Sqlite;
using Dapper;
using CardShop.Repositories.Models;
using System.Data;
using CardShop.Extensions;


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
                        WHERE UserId = @UserId
                        AND Count > 0",
                        new
                        {
                            UserId = userId
                        });

            return inventory.AsList();
        }

        public async Task RemoveEmptyUserInventory(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var inventory = await dbConnection.ExecuteAsync($@"
                        DELETE
                        FROM Inventory
                        WHERE Count < 1
                        AND UserId = @UserId", new { UserId = userId});
        }

        public async Task<bool> UpsertInventory(List<Inventory> inventoryItems)
        {

            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();

            var success = await UpsertInventory(inventoryItems, dbConnection, transaction);

            if (!success)
            {
                transaction.Rollback();
                return success;
            }

            transaction.Commit();
            return success;
        }

        public async Task<bool> UpsertMultipleInventory(List<List<Inventory>> inventoryRequests)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();

            foreach (var inventoryRequest in inventoryRequests)
            {
                var success = await UpsertInventory(inventoryRequest, dbConnection, transaction);

                if (!success)
                {
                    transaction.Rollback();
                    return success;
                }
            }

            transaction.Commit();
            return true;

        }

        private async Task<bool> UpsertInventory(List<Inventory> inventoryItems, SqliteConnection dbConnection, SqliteTransaction transaction)
        {
            var returnInventory = new List<Inventory>();

            inventoryItems = inventoryItems.Consolidate();

            try
            {

                // Perform the upsert operation (insert or replace)
                foreach (var item in inventoryItems)
                {
                    
                    await dbConnection.ExecuteAsync(@"
            INSERT OR REPLACE INTO Inventory (ProductCode, SetCode, UserId, Count) 
            VALUES (@ProductCode, @SetCode, @UserId, @Count)", new { ProductCode = item.ProductCode, SetCode = item.SetCode.ToString(), UserId = item.UserId, Count = item.Count });
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error occurred while trying to insert or update inventory.", inventoryItems.ToArray());
                return false;
            }

            return true;
        } 

        public async Task<bool> InsertInventory(string productCode, string setCode, int count, string userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = await dbConnection.ExecuteAsync($@"
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

        public bool DeleteUserInventory(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var updatedRowCount = dbConnection.ExecuteScalar<int>($@"
                        DELETE FROM Inventory WHERE UserId = @UserId",

                        new
                        {
                            UserId = userId
                        });

            return true;
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
