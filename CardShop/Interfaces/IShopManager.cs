using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IShopManager
    {
        void Initialize();
        Task<List<InventoryItemInternal>> GetVerboseShopInventory(bool includeOutOfStock);
        bool ClearShopInventory();

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestedItems"></param>
        /// <returns>Purchased Items, Total Cost, Remaining Balance, Error Message</returns>
        Task<(List<InventoryItemInternal>, decimal, decimal, string)> PurchaseInventory(string username, List<ProductReference> requestedItems);
        Task<bool> AllotStartingInventoryToUser(string userName);
    }
}
