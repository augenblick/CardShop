using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IInventoryManager
    {
        Task<List<Inventory>> AddInventory(List<KeyValuePair<Product, int>> products, int userId);
        Task<List<Inventory>> GetUserInventory(int userId);
        List<InventoryItem> InventoryItemsFromInventory(List<Inventory> inventory);
        bool ClearUserInventory(int userId);
    }
}
