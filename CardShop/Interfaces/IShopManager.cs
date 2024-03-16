using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IShopManager
    {
        void Initialize();
        Task<List<InventoryItem>> GetVerboseShopInventory(bool includeOutOfStock);
        bool ClearShopInventory();
        Task<(List<InventoryItem>, string)> PurchaseInventory(int userId, List<PurchaseRequest> requestedItems);
    }
}
