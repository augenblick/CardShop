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
            var existingInventory = await GetShopInventory();

            var availableSets = _cardProductBuilder.GetAvailableCardSets(new List<string> { "full" });
            var requestList = new List<(ProductType, CardSetCode, int)>();
            const int maxBoxCount = 5;
            const int maxPackCount = 30;

            foreach (var set in availableSets)
            {
                var matchingBoxInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.BoosterBox);
                var matchingPackInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.BoosterPack);

                var currentBoxCount = matchingBoxInventory.Sum(x => x.Count);
                var currentPackCount = matchingPackInventory.Sum(x => x.Count);

                if (matchingBoxInventory == null)
                {
                    requestList.Add((ProductType.BoosterBox, set, maxBoxCount));
                }
                else if (currentBoxCount < (maxBoxCount * 0.3))
                {
                    requestList.Add((ProductType.BoosterBox, set, maxBoxCount - currentBoxCount));
                }

                if (matchingPackInventory == null)
                {
                    requestList.Add((ProductType.BoosterPack, set, maxPackCount));
                }
                else if (currentPackCount < (maxPackCount * 0.3))
                {
                    requestList.Add((ProductType.BoosterPack, set, maxPackCount - currentPackCount));
                }


            }


            var productList = new List<KeyValuePair<Product, int>>();

            foreach (var request in requestList)
            {
                productList.AddRange(_cardProductBuilder.GetProductsByProductType(request.Item1, request.Item2, request.Item3));
            }

            var inventoryAdd = await _inventoryManager.AddInventory(productList, _shopKeeperUserId);
        }

        public async Task<List<InventoryItem>> GetShopInventory()
        {
            var returnInventory = new List<InventoryItem>();

            var shopInventory = await _inventoryManager.GetUserInventory(_shopKeeperUserId);

            return _inventoryManager.InventoryItemsFromInventory(shopInventory);
        }
    }
}
