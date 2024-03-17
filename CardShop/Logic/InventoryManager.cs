using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;
using System.Collections.Generic;
using System.Linq;

namespace CardShop.Logic
{
    

    public class InventoryManager : IInventoryManager
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IUserManager _userManager;
        private readonly ILogger _logger;

        public InventoryManager(IInventoryRepository inventoryRepository, ICardProductBuilder cardProductBuilder, ILogger<InventoryManager> logger, IUserManager userManager)
        {
            _inventoryRepository = inventoryRepository;
            _cardProductBuilder = cardProductBuilder;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            return await _inventoryRepository.GetUserInventory(userId);
        }

        public async Task<bool> UpdateMultipleInventory(List<List<Inventory>> inventoryRequests)
        {
            return await _inventoryRepository.UpsertMultipleInventory(inventoryRequests);
        }

        public async Task<bool> AddInventory(List<KeyValuePair<Product, int>> productsToAdd, int userId)
        {
            if (productsToAdd == null || productsToAdd.Count < 1)
            {
                return true;
            }

            var groupedProductToAdd = productsToAdd
            .GroupBy(
                pair => new { pair.Key.Code, pair.Key.SetCode }, // keySelector
                pair => pair.Value, // elementSelector
                (key, quantities) => new KeyValuePair<Product, int>(new Product { Code = key.Code, SetCode = key.SetCode }, quantities.Sum()) // resultSelector
            ).ToList();

            var userInventory = await _inventoryRepository.GetUserInventory(userId);

            var inventoryToAdd = new List<Inventory>();

            
            foreach (var product in groupedProductToAdd)
            {
                var count = product.Value;

                if (userInventory.Count > 0)
                {
                    var existing = userInventory.FirstOrDefault(x => x.SetCode == product.Key.SetCode && x.ProductCode == product.Key.Code);

                    if (existing != null)
                    {
                        count += existing.Count;
                    }
                }

                inventoryToAdd.Add(
                    new Inventory{
                        SetCode = product.Key.SetCode,
                        ProductCode = product.Key.Code,
                        UserId = userId,
                        Count = count
                    });
            }
            
            return await _inventoryRepository.UpsertInventory(inventoryToAdd);
        }

        public List<InventoryItem> InventoryItemsFromInventory(List<Inventory> inventory)
        {
            var returnInventory = new List<InventoryItem>();

            foreach (var inventoryItem in inventory)
            {
                var product = _cardProductBuilder.GetProduct(inventoryItem);

                if (product == null)
                {
                    _logger.LogError($"unable to find the product corresponding with a given inventory item! InventoryId: {inventoryItem.InventoryId}, ProductCode: {inventoryItem.ProductCode}");
                    break;
                }

                returnInventory.Add(new InventoryItem
                {
                    Product = product,
                    Count = inventoryItem.Count
                });
            }

            return returnInventory;
        }

        public bool ClearUserInventory(int userId)
        {
            return _inventoryRepository.DeleteUserInventory(userId);
        }

        public async Task<(List<InventoryItem>, string)> OpenInventoryProducts(int userId, List<ProductReference> itemsToOpen)
        {
            var returnList = new List<InventoryItem>();
            var errorMessage = string.Empty;

            if (itemsToOpen.Any(x => x.Count < 0))
            {
                errorMessage = $"Negative counts not allowed!";
                _logger.LogError(errorMessage);
                return (returnList, errorMessage);
            }

            // group like items
            var groupedRequest = itemsToOpen
                .GroupBy(
                    item => new { item.ProductCode },
                    (key, group) => new ProductReference
                    {
                        ProductCode = key.ProductCode,
                        Count = group.Sum(item => item.Count)
                    })
                .ToList();

            // user exists?
            var user = _userManager.GetUser(userId);
            if (user == null)
            {
                errorMessage = $"User '{userId}' not found!";
                _logger.LogError(errorMessage);
                return (returnList, errorMessage);
            }

            var userInventory = await _inventoryRepository.GetUserInventory(userId);

            var matchingInventoryItems = new List<Inventory>();
            var existingUserInventoryToModify = new List<Inventory>();
            var productsToAddToUserInventory = new List<Inventory>();

            foreach(var item in groupedRequest)
            {
                var matchingInventory = userInventory.FirstOrDefault(x => x.ProductCode == item.ProductCode);

                if (matchingInventory == null)
                {
                    errorMessage = $"requested product {item.ProductCode} not found in inventory!";
                    _logger.LogError(errorMessage);
                    return (returnList, errorMessage);
                }

                if (matchingInventory.Count < item.Count)
                {
                    errorMessage = $"Not enough of requested product {item.ProductCode} in inventory!  {item.Count}x requested, only {matchingInventory.Count}x found.";
                    _logger.LogError(errorMessage);
                    return (returnList, errorMessage);
                }

                matchingInventoryItems.Add(matchingInventory);
            }

            foreach(var item in matchingInventoryItems)
            {
                var existingInventoryCount = item.Count;

                var requestedCount = groupedRequest.First(x => x.ProductCode == item.ProductCode).Count;

                if (requestedCount < 1) { continue; }

                // modify existing count
                existingUserInventoryToModify.Add(new Inventory
                {
                    ProductCode = item.ProductCode,
                    SetCode = item.SetCode,
                    UserId = userId,
                    Count = existingInventoryCount - requestedCount
                });

                var product = _cardProductBuilder.GetProduct(item.ProductCode);

                if (product == null)
                {
                    errorMessage = $"Product '{item.ProductCode}' could not be acquired!";
                    _logger.LogError(errorMessage);
                    return (returnList, errorMessage);
                }

                // TODO: move this check into CardProductBuilder?
                if (product.ProductType != ProductType.BoosterPack) 
                {
                    // open once and apply count to inventory listing
                    // TODO: This method assumes homogeneous  contents-- need to refactor to support Heterogeneous contents
                    // This may be unnecessary as we consolidate the list at the end before submitting to the database
                    var productContents = _cardProductBuilder.OpenProduct(product);

                    if (productContents == null || productContents.FirstOrDefault() == null)
                    {
                        errorMessage = $"Couldn't open product '{product.Code}'!";
                        _logger.LogError(errorMessage);
                        return (returnList, errorMessage);
                    }

                    // get matching inventory
                    var existingMatchingInventory = userInventory.Where(x => x.ProductCode == productContents.First().Code).ToList();

                    productsToAddToUserInventory.AddRange(productContents.Select(x => new Inventory
                    {
                        Count = requestedCount + existingMatchingInventory.Count,
                        ProductCode = x.Code,
                        UserId = userId,
                        SetCode = x.SetCode
                    }));
                }
                else
                {
                    // random draw for each pack
                    for (int i = 0; i < requestedCount; i++)
                    {
                        var productContents = _cardProductBuilder.OpenProduct(product);

                        productsToAddToUserInventory.AddRange(productContents.Select(x => new Inventory
                        {
                            Count = 1,
                            ProductCode = ((Card)x).Code,   // TODO: cleaner way to do this?
                            UserId = userId,
                            SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)x).SetCode)    // TODO: cleaner way to do this?
                        }));
                    }
                }
            }


            var groupedAddList = productsToAddToUserInventory
                .GroupBy(
                    item => new { item.ProductCode, item.SetCode },
                    (key, group) => new Inventory
                    {
                        ProductCode = key.ProductCode,
                        SetCode = key.SetCode,
                        Count = group.Sum(item => item.Count),
                        UserId = userId
                    })
                .ToList();


            // TODO: do we need to do this if we grouped the requested items?
            var groupedModifyList = existingUserInventoryToModify
                .GroupBy(
                    item => new { item.ProductCode, item.SetCode },
                    (key, group) => new Inventory
                    {
                        ProductCode = key.ProductCode,
                        SetCode = key.SetCode,
                        Count = group.Sum(item => item.Count),
                        UserId = userId
                    })
                .ToList();

            // add new items to user inventory
            // remove requested items from inventory
            var successfulInsert = await _inventoryRepository.UpsertMultipleInventory(new List<List<Inventory>> { groupedAddList, groupedModifyList });

            if (!successfulInsert)
            {
                errorMessage = $"database insert was unsuccessful!";
                _logger.LogError(errorMessage);
            }
            else
            {
                returnList = InventoryItemsFromInventory(productsToAddToUserInventory);
                // TODO: remove inventory with count 0
            }

            return (returnList, errorMessage);
        }
    }
}
