using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
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
        private readonly IUserManager _userManager;

        public ShopManager(ICardProductBuilder cardProductBuilder, IInventoryManager inventoryManager, ILogger<ShopManager> logger, IUserManager userManager)
        {
            _cardProductBuilder = cardProductBuilder;
            _inventoryManager = inventoryManager;
            _logger = logger;
            _userManager = userManager;
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

        public async Task<List<InventoryItem>> PurchaseInventory(int userId, List<PurchaseRequest> requestedItems)
        {
            var user = await _userManager.GetUser(userId);

            if (user == null)
            {
                // TODO: send back error message
                _logger.LogError($"user with Id '{userId}' not found!");
                return null;
            }

            var totalCost = 0.0M;

            // TODO: ACQUIRE LOCK

            var shopInventory = await GetShopInventory();


            // check that requested inventory exists
            foreach(var item in requestedItems)
            {
                var matchingInventory = shopInventory.FirstOrDefault(x => x.InventoryId == item.InventoryId && x.Count >= item.Count);

                if (matchingInventory == null)
                {
                    // TODO: send back error message
                    _logger.LogError($"Request inventory '{item.InventoryId}' of count '{item.Count}' not found!");
                    return null;
                }

                totalCost += matchingInventory.Product.CostPer * item.Count;
            }

            // does user have the funds?
            if (user.Balance < totalCost)
            {
                // TODO: send back error message
                _logger.LogError($"User {userId} doesn't have enough money for this purchace.  User has ${user.Balance} of the required amount ${totalCost}");
                return null;
            }

            // create list for DB update
            var shopInventoryToUpdate = new List<Inventory>();
            var userInventoryToUpdate = new List<Inventory>();
            foreach (var item in requestedItems)
            {
                var matchingInventory = shopInventory.First(x => x.InventoryId == item.InventoryId && x.Count >= item.Count);

                shopInventoryToUpdate.Add(new Inventory
                {
                    ProductCode = matchingInventory.Product.Code,
                    SetCode = matchingInventory.Product.SetCode,
                    UserId = _shopKeeperUserId,
                    Count = matchingInventory.Count - item.Count
                });

                userInventoryToUpdate.Add(new Inventory
                {
                    ProductCode = matchingInventory.Product.Code,
                    SetCode = matchingInventory.Product.SetCode,
                    UserId = userId,
                    Count = item.Count
                });
            }

            var isSuccessfulInsert = await _inventoryManager.UpdateMultipleInventory(new List<List<Inventory>> { shopInventoryToUpdate, userInventoryToUpdate });

            var balanceUpdated = await _userManager.SetUserBalance(userId, user.Balance - totalCost);
            if (!balanceUpdated)
            {
                _logger.LogError($"User balance wasn't updated!");
            }

            // TODO: RELEASE LOCK

            if (!isSuccessfulInsert)
            {
                // TODO: send back error message
                _logger.LogError($"An error occurred during the insert.");
                return new List<InventoryItem>();
            }

            return _inventoryManager.InventoryItemsFromInventory(userInventoryToUpdate);
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
