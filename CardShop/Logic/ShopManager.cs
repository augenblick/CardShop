using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace CardShop.Logic
{
    public class ShopManager : IShopManager
    {
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IInventoryManager _inventoryManager;
        private const string _shopKeeperUserName = "ShopKeeper";
        private const int _shopKeeperUserId = 0;
        private readonly ILogger _logger;

        public ShopManager(ICardProductBuilder cardProductBuilder, IInventoryManager inventoryManager, ILogger<ShopManager> logger)
        {
            _cardProductBuilder = cardProductBuilder;
            _inventoryManager = inventoryManager;
            _logger = logger;
        }

        public async void Initialize()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

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
                var product = _cardProductBuilder.GetProductByProductType(request.Item1, request.Item2);
                productList.Add(new KeyValuePair<Product, int>(product, request.Item3));
            }

            var inventoryAdd = await _inventoryManager.AddInventory(productList, _shopKeeperUserId);

            stopWatch.Stop();

            _logger.LogInformation($">>> Shop Initialization took '{stopWatch.ElapsedMilliseconds}'ms.");
        }

        public async Task<List<InventoryItem>> GetShopInventory()
        {
            var returnInventory = new List<InventoryItem>();

            var shopInventory = await _inventoryManager.GetUserInventory(_shopKeeperUserId);

            return _inventoryManager.InventoryItemsFromInventory(shopInventory);
        }
        public bool ClearShopInventory()
        {
            return _inventoryManager.ClearUserInventory(_shopKeeperUserId);
        }
    }
}
