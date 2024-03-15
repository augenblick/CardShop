﻿using CardShop.Enums;
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

        private static readonly Mutex _shopManagerMutex = new Mutex();

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

        public async Task<(List<InventoryItem>, string)> PurchaseInventory(int userId, List<PurchaseRequest> requestedItems)
        {
            var errorMessage = string.Empty;
            var returnList = new List<InventoryItem>();
            
            var user = await _userManager.GetUser(userId);

            if (user == null)
            {
                errorMessage = $"user with Id '{userId}' not found!";
                _logger.LogError(errorMessage);
                return (returnList, errorMessage);
            }

            var totalCost = 0.0M;

            // create lists used for upcoming DB update
            var shopInventoryToUpdate = new List<Inventory>();
            var userInventoryToUpdate = new List<Inventory>();

            var isSuccessfulInsert = false;

            try
            {
                // acquire lock
                _shopManagerMutex.WaitOne();

                var shopInventory = await GetShopInventory();


                // check that requested inventory exists
                foreach (var item in requestedItems)
                {
                    if (item.Count < 1) { continue; }

                    var matchingInventory = shopInventory.FirstOrDefault(x => x.InventoryId == item.InventoryId && x.Count >= item.Count);

                    if (matchingInventory == null)
                    {
                        errorMessage = $"Request inventory '{item.InventoryId}' of count '{item.Count}' not found!";
                        _logger.LogError(errorMessage);
                        return (returnList, errorMessage);
                    }

                    totalCost += matchingInventory.Product.CostPer * item.Count;
                }

                // does user have the funds?
                if (user.Balance < totalCost)
                {
                    errorMessage = $"User {userId} doesn't have enough money for this purchace.  User has ${user.Balance} of the required amount ${totalCost}";
                    _logger.LogError(errorMessage);
                    return (returnList, errorMessage);
                }

                
                foreach (var item in requestedItems)
                {
                    if (item.Count < 1) { continue; }

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

                isSuccessfulInsert = await _inventoryManager.UpdateMultipleInventory(new List<List<Inventory>> { shopInventoryToUpdate, userInventoryToUpdate });

                var balanceUpdated = await _userManager.SetUserBalance(userId, user.Balance - totalCost);
                if (balanceUpdated)
                {
                    _logger.LogError($"User balance wasn't updated!");
                }
            }
            finally
            {
                // release lock
                _shopManagerMutex.ReleaseMutex();
            }

            if (!isSuccessfulInsert)
            {
                errorMessage = $"An error occurred during the insert.";
                _logger.LogError(errorMessage);
                return (returnList, errorMessage);
            }

            return (_inventoryManager.InventoryItemsFromInventory(userInventoryToUpdate), errorMessage);
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
