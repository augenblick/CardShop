using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IInventoryManager
    {
        Task<bool> AddInventory(List<KeyValuePair<Product, int>> productsToAdd, int userId);
        Task<List<Inventory>> GetUserInventory(int userId);
        List<InventoryItem> InventoryItemsFromInventory(List<Inventory> inventory);
        bool ClearUserInventory(int userId);
    }
}
