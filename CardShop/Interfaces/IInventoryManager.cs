using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IInventoryManager
    {
        Task<bool> AddInventory(List<KeyValuePair<Product, int>> products, int userId);
        Task<List<Inventory>> GetUserInventory(int userId);
    }
}
