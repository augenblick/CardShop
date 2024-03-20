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

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestedItems"></param>
        /// <returns>Purchased Items, Total Cost, Remaining Balance, Error Message</returns>
        Task<(List<InventoryItem>, decimal, decimal, string)> PurchaseInventory(int userId, List<ProductReference> requestedItems);
    }
}
