using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IShopManager
    {
        void Initialize();
        List<Inventory> GetShopInventory();
    }
}
