using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IInventoryManager
    {
        Task<bool> AddInventory(List<InventoryItemInternal> productsToAdd, int userId);
        Task<bool> UpdateMultipleInventory(List<List<Inventory>> inventoryRequests);
        Task<List<Inventory>> GetUserInventory(int userId);
        Task<List<InventoryItemInternal>> GetUserInventoryItems(int userId, ProductType? productTypeFilter = null);
        Task<(List<InventoryItemInternal>, string)> OpenInventoryProducts(int userId, List<ProductReference> itemsToOpen);
        List<InventoryItemInternal> InventoryItemsFromInventory(List<Inventory> inventory);
        bool ClearUserInventory(int userId);
    }
}
