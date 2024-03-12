using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using System.Reflection.Metadata.Ecma335;

namespace CardShop.Logic
{
    public class ShopManager : IShopManager
    {
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IInventoryManager _inventoryManager;
        private const string _shopKeeperUserName = "ShopKeeper";
        private const int _shopKeeperUserId = 0;

        public ShopManager(ICardProductBuilder cardProductBuilder, IInventoryManager inventoryManager)
        {
            _cardProductBuilder = cardProductBuilder;
            _inventoryManager = inventoryManager;
        }

        public async void Initialize()
        {
            var requestList = new List<(ProductType, CardSetCode, int)>
            {
                // we will eventually build this dynamically
                (ProductType.BoosterBox, CardSetCode.Premiere, 5),
            };

            var productList = new List<KeyValuePair<Product, int>>();

            foreach (var request in requestList)
            {
                productList.AddRange(_cardProductBuilder.GetProductsByProductType(request.Item1, request.Item2, request.Item3));
            }

            var inventoryAdd = await _inventoryManager.AddInventory(productList, _shopKeeperUserId);
        }

        public async Task<List<Inventory>> GetShopInventory()
        {
            var shopInventory = await _inventoryManager.GetUserInventory(_shopKeeperUserId);
            return shopInventory;
        }
    }
}
