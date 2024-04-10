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

            var productList = BuildProductOrder();

            var clearInventoryResult = _inventoryManager.ClearUserInventory(_shopKeeperUserId);
            var inventoryAdd = await _inventoryManager.AddInventory(productList, _shopKeeperUserId);

            stopWatch.Stop();

            _logger.LogInformation($">>> Shop Initialization took '{stopWatch.ElapsedMilliseconds}'ms.");
        }

        private List<InventoryItemInternal> BuildProductOrder()
        {
            var buildSimple = _configuration.GetValue<bool>("BuildSimpleShopInventory", true);

            decimal availableFunds = 500.0M;

            // TODO:
            // get data used to set order-per-product probabilities
            //// recent orders
            //// current date
            //// recent purchases
            //// recent requests

            var availableProducts = _cardProductBuilder.GetAllExistingProducts().Select(x => new InventoryItemInternal { Product = x, Count = 1 }).AsList();

            if (availableProducts == null)
            {
                _logger.LogError($"No available products for the shop to order!");
            }

            var orderList = new List<InventoryItemInternal>();

            if (!buildSimple)
            {
                foreach (var product in availableProducts)
                {
                    // TODO build initial purchase likelihood for each product
                }

                // insert products into pool
                var productPool = new Pool<Product>("InitialProductPool", availableProducts.Select(x => new KeyValuePair<Product, int>(x.Product, x.Count)).AsList());

                do
                {
                    var chosenProduct = productPool.Draw();

                    if (chosenProduct == null)
                    {
                        _logger.LogError($"Unable to draw a product from the product pool.");
                        break;
                    }

                    var cost = chosenProduct.CostPer;
                    var countToAdd = 1;

                    if (chosenProduct.ProductType == ProductType.BoosterPack)
                    {
                        cost *= 10;
                        countToAdd *= 10;
                    }

                    availableFunds -= cost;

                    orderList.Add(new InventoryItemInternal { Product = chosenProduct, Count = countToAdd });
                }
                while (availableFunds > 0.0M);
            }
            else
            {
                var purchasableItems = availableProducts.Where(x => x.Product.IsPurchasable);
                foreach (var product in purchasableItems)
                {
                    var orderCountDecimal = 80M / product.Product.CostPer;

                    var orderCount = (int)Math.Floor(orderCountDecimal);

                    product.Count = orderCount < 1 ? 1 : orderCount;
                }

                orderList = purchasableItems.AsList();
            }

            return orderList;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestedItems"></param>
        /// <returns>Purchased Items, Total Cost, Remaining Balance, Error Message</returns>
        public async Task<(List<InventoryItemInternal>, decimal, decimal, string)> PurchaseInventory(string username, List<ProductReference> requestedItems)
        {
            var errorMessage = string.Empty;
            var returnList = new List<InventoryItemInternal>();
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
            var startingProductList = new List<InventoryItemInternal>
            {
                new InventoryItemInternal
                {
                    Product = _cardProductBuilder.GetProduct("013s", CardSetCode.PremiereTwoPlayer), Count = 1
                }
            };

            return await _inventoryManager.AddInventory(startingProductList, user.UserId);
        }

        public async Task<List<InventoryItemInternal>> GetVerboseShopInventory(bool includeOutOfStock)
        {
            var returnItems = new List<InventoryItemInternal>();

            var allPossibleInventory = _cardProductBuilder.GetAllExistingProducts().Where(y => y.IsPurchasable).Select( x => new InventoryItemInternal
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

        private async Task<List<InventoryItemInternal>> GetShopInventory()
        {
            var returnInventory = new List<InventoryItemInternal>();

            var shopInventory = await _inventoryManager.GetUserInventory(_shopKeeperUserId);

            return _inventoryManager.InventoryItemsFromInventory(shopInventory);
        }
    }
}
