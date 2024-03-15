using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IInventoryRepository
    {
        Task<List<Inventory>> GetUserInventory(int userId);
        Task<bool> UpsertMultipleInventory(List<List<Inventory>> inventoryRequests);
        Task<bool> UpsertInventory(List<Inventory> inventoryItems);
        Task<bool> InsertInventory(string productCode, string setCode, int count, string userId);
        bool DeleteUserInventory(int userId);
    }
}
