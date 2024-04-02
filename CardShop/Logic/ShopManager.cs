using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;
using Dapper;
using System.Diagnostics;
using System.Globalization;

namespace CardShop.Logic
{
    public class ShopManager : IShopManager
    {
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IInventoryManager _inventoryManager;
        private int _shopKeeperUserId;
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;
        private readonly IConfiguration _configuration;

        private static readonly Mutex _shopManagerMutex = new Mutex();

        public ShopManager(ICardProductBuilder cardProductBuilder, IInventoryManager inventoryManager, ILogger<ShopManager> logger, IUserManager userManager, IConfiguration configuration)
        {
            _cardProductBuilder = cardProductBuilder;
            _inventoryManager = inventoryManager;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async void Initialize()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var shopUser = await _userManager.GetUser(_configuration.GetValue<string>("ShopkeeperUsername") ?? "ShopKeeper");
            if (string.IsNullOrWhiteSpace(shopUser.Username))
            {
                _logger.LogError("The Shopkeeper user could not be obtained when initializing the shop.");
            }

            _shopKeeperUserId = shopUser.UserId;

            var existingInventory = await GetShopInventory();

            var availableSets = _cardProductBuilder.GetAvailableCardSets(new List<string> { "full", "premium" });
            var requestList = new List<(ProductType, CardSetCode, int)>();
            const int maxBoxCount = 5;
            const int maxPackCount = 30;
            const int maxDeckCount = 10;
            const int maxSealedDeckCount = 5;
            const int maxMiscCount = 10;

            foreach (var set in availableSets)
            {
                var matchingBoxInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.BoosterBox);
                var matchingPackInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.BoosterPack);
                var matchingDeckInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.StarterDeck);
                var matchingSealedDeckInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.SealedDeck);
                var matchingMiscInventory = existingInventory.Where(x => x.Product.SetCode == set && x.Product.ProductType == ProductType.Miscellaneous);

                var currentBoxCount = matchingBoxInventory.Sum(x => x.Count);
                var currentPackCount = matchingPackInventory.Sum(x => x.Count);
                var currentDeckCount = matchingDeckInventory.Sum(x => x.Count);
                var currentSealedDeckCount = matchingDeckInventory.Sum(x => x.Count);
                var currentMiscCount = matchingDeckInventory.Sum(x => x.Count);

                if (currentBoxCount < (maxBoxCount * 0.3))
                {
                    requestList.Add((ProductType.BoosterBox, set, maxBoxCount - currentBoxCount));
                }

                if (currentPackCount < (maxPackCount * 0.3))
                {
                    requestList.Add((ProductType.BoosterPack, set, maxPackCount - currentPackCount));
                }

                if (currentDeckCount < (maxDeckCount * 0.3))
                {
                    requestList.Add((ProductType.StarterDeck, set, maxDeckCount - currentPackCount));
                }

                if (currentSealedDeckCount < (maxSealedDeckCount * 0.3))
                {
                    requestList.Add((ProductType.SealedDeck, set, maxSealedDeckCount - currentPackCount));
                }

                if (currentMiscCount < (maxMiscCount * 0.3))
                {
                    requestList.Add((ProductType.Miscellaneous, set, maxMiscCount - currentPackCount));
                }
            }

            var productList = new List<KeyValuePair<Product, int>>();

            foreach (var request in requestList)
            {
                var product = _cardProductBuilder.GetProductByProductType(request.Item1, request.Item2);

                if (product != null && product.SetCode != CardSetCode.undefined)
                {
                    productList.Add(new KeyValuePair<Product, int>(product, request.Item3));
                }
            }

            var inventoryAdd = await _inventoryManager.AddInventory(productList, _shopKeeperUserId);

            stopWatch.Stop();

            _logger.LogInformation($">>> Shop Initialization took '{stopWatch.ElapsedMilliseconds}'ms.");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestedItems"></param>
        /// <returns>Purchased Items, Total Cost, Remaining Balance, Error Message</returns>
        public async Task<(List<InventoryItem>, decimal, decimal, string)> PurchaseInventory(string username, List<ProductReference> requestedItems)
        {
            var errorMessage = string.Empty;
            var returnList = new List<InventoryItem>();
            var totalCostFinal = 0.0M;  // should remain 0.0 until money actually removed from account
            var userBalance = 0.0M;

            var user = await _userManager.GetUser(username);

            if (user == null)
            {
                errorMessage = $"user '{username}' not found!";
                _logger.LogError(errorMessage);
                return (returnList, totalCostFinal, userBalance, errorMessage);
            }

            var userId = user.UserId;

            userBalance = user.Balance;
            var totalCost = 0.0M;

            // create lists used for upcoming DB update
            var shopInventoryToUpdate = new List<Inventory>();
            var userInventoryToUpdate = new List<Inventory>();
            var purchasedInventoryToReturn = new List<Inventory>();

            var isSuccessfulInsert = false;

            try
            {
                // acquire lock
                _shopManagerMutex.WaitOne();

                var shopInventory = await GetShopInventory();
                var existingUserInventory = await _inventoryManager.GetUserInventory(userId);

                // check that requested inventory exists
                foreach (var item in requestedItems)
                {

                    if (item.Count < 1) { continue; }

                    var matchingInventory = shopInventory.FirstOrDefault(x => x.Product.Code == item.ProductCode && x.Count > 0);
                    if (matchingInventory == null)
                    {
                        errorMessage = $"Requested Product '{item.ProductCode}' not in stock!";
                        _logger.LogError(errorMessage);
                        return (returnList, totalCostFinal, userBalance, errorMessage);
                    }

                    if (matchingInventory.Count < item.Count)
                    {
                        errorMessage = $"Requested {item.Count}x of Product '{item.ProductCode}', but only {matchingInventory.Count}x are in stock!";
                        _logger.LogError(errorMessage);
                        return (returnList, totalCostFinal, userBalance, errorMessage);
                    }

                    totalCost += matchingInventory.Product.CostPer * item.Count;
                }

                var cultureInfo = new CultureInfo("en-US"); // Specify the culture (e.g., US English)

                // does user have the funds?
                if (user.Balance < totalCost)
                {
                    errorMessage = $"User '{username}' doesn't have enough money for this purchase.  User has {user.Balance.ToString("C", cultureInfo)} of the required cost of {totalCost.ToString("C", cultureInfo)}";
                    _logger.LogError(errorMessage);
                    return (returnList, totalCostFinal, userBalance, errorMessage);
                }

                foreach (var item in requestedItems)
                {
                    var requestedCount = item.Count;

                    if (item.Count < 1) { continue; }

                    var matchingInventory = shopInventory.First(x => x.Product.Code == item.ProductCode && x.Count >= item.Count);
                    var matchingUserInventory = existingUserInventory.FirstOrDefault(x => x.ProductCode == item.ProductCode);

                    // using item.Count in any compound statement was causing issues, so let's break it down
                    var matchingCount = matchingUserInventory?.Count ?? 0;
                    var newCount = matchingCount + requestedCount;

                    shopInventoryToUpdate.Add(new Inventory
                    {
                        ProductCode = matchingInventory.Product.Code,
                        SetCode = matchingInventory.Product.SetCode,
                        UserId = _shopKeeperUserId,
                        Count = matchingInventory.Count - requestedCount
                    });

                    userInventoryToUpdate.Add(new Inventory
                    {
                        ProductCode = matchingInventory.Product.Code,
                        SetCode = matchingInventory.Product.SetCode,
                        UserId = userId,
                        Count = newCount
                    });

                    purchasedInventoryToReturn.Add(new Inventory
                    {
                        ProductCode = matchingInventory.Product.Code,
                        SetCode = matchingInventory.Product.SetCode,
                        UserId = userId,
                        Count = requestedCount
                    });
                }

                isSuccessfulInsert = await _inventoryManager.UpdateMultipleInventory(new List<List<Inventory>> { shopInventoryToUpdate, userInventoryToUpdate });

                if (!isSuccessfulInsert)
                {
                    errorMessage = $"An error occurred during the insert.";
                    _logger.LogError(errorMessage);
                    return (returnList, totalCostFinal, userBalance, errorMessage);
                }

                var balanceUpdated = await _userManager.SetUserBalance(userId, user.Balance - totalCost);
                if (balanceUpdated)
                {
                    userBalance = user.Balance - totalCost;
                    totalCostFinal = totalCost;
                }
                else 
                {
                    _logger.LogError($"User balance wasn't updated!");
                }
            }
            finally
            {
                // release lock
                _shopManagerMutex.ReleaseMutex();
            }

            return (_inventoryManager.InventoryItemsFromInventory(purchasedInventoryToReturn), totalCostFinal, userBalance, errorMessage);
        }

        public async Task<bool> AllotStartingInventoryToUser(string userName)
        {
            var user = await _userManager.GetUser(userName);

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return false;
            }

            // TODO: rework this, incl. probably defining starting items elsewhere
            var startingProductList = new List<KeyValuePair<Product, int>>
            {
                new KeyValuePair<Product, int>(_cardProductBuilder.GetProduct("013s", CardSetCode.PremiereTwoPlayer), 1)
            };

            return await _inventoryManager.AddInventory(startingProductList, user.UserId);
        }

        public async Task<List<InventoryItem>> GetVerboseShopInventory(bool includeOutOfStock)
        {
            var returnItems = new List<InventoryItem>();

            var allPossibleInventory = _cardProductBuilder.GetAllExistingProducts().Where(y => y.IsPurchasable).Select( x => new InventoryItem
            {
                Count = 0,
                Product = x
            });

            var shopInventory = await _inventoryManager.GetUserInventory(_shopKeeperUserId);

            // var completeInventory = new List<InventoryItem>();

            foreach(var possibleInventory in allPossibleInventory)
            {
                var matchingExistingInventory = shopInventory.FirstOrDefault(x => x.ProductCode == possibleInventory.Product.Code);

                if (matchingExistingInventory != null)
                {
                    possibleInventory.Count = matchingExistingInventory.Count;
                }

                returnItems.Add(possibleInventory);
            }

            if (!includeOutOfStock)
            {
                return returnItems.Where(x => x.Count > 0).AsList();
            }

            return returnItems.AsList();
        }

        public bool ClearShopInventory()
        {
            return _inventoryManager.ClearUserInventory(_shopKeeperUserId);
        }

        private async Task<List<InventoryItem>> GetShopInventory()
        {
            var returnInventory = new List<InventoryItem>();

            var shopInventory = await _inventoryManager.GetUserInventory(_shopKeeperUserId);

            return _inventoryManager.InventoryItemsFromInventory(shopInventory);
        }
    }
}
