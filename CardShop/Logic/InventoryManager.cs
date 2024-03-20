using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;
using Dapper;
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
            var uncommittedReturnList = new List<Inventory>();
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

            var inventoryMatchingItemsToOpen = new List<Inventory>();
            var existingUserInventoryToUpdate = new List<Inventory>();
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

                inventoryMatchingItemsToOpen.Add(matchingInventory);
            }

            foreach(var item in inventoryMatchingItemsToOpen)
            {
                var existingInventoryCount = item.Count;

                var requestedCount = groupedRequest.First(x => x.ProductCode == item.ProductCode).Count;

                if (requestedCount < 1) { continue; }

                // save record to modify existing inventory count at the end
                existingUserInventoryToUpdate.Add(new Inventory
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

                // TODO: move this check and some logic into CardProductBuilder?
                if (product is not BoosterPack)
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

                    // get inventory that matches opened items
                    var existingInventoryMatchingOpenedItems = userInventory.FirstOrDefault(x => x.ProductCode == productContents.First().Product.Code);

                    productsToAddToUserInventory.AddRange(productContents.Select(x => new Inventory
                    {
                        // add existing count to newly added product count
                        Count = (x.Count * requestedCount) + (existingInventoryMatchingOpenedItems?.Count ?? 0),
                        ProductCode = x.Product.Code,
                        UserId = userId,
                        SetCode = x.Product.SetCode
                    }));


                    // maintain separate list for return to consumer (which doesn't include counts of matching inventory) 
                    uncommittedReturnList.AddRange(productContents.Select(x => new Inventory
                    {
                        Count = (x.Count * requestedCount),
                        ProductCode = x.Product.Code,
                        UserId = userId,
                        SetCode = x.Product.SetCode
                    }));
                }
                else
                {
                    // random draw for each pack
                    for (int i = 0; i < requestedCount; i++)
                    {
                         var productContentsAgain = _cardProductBuilder.OpenProduct(product);

                        if (productContentsAgain == null || productContentsAgain.FirstOrDefault() == null)
                        {
                            errorMessage = $"Couldn't open product '{product.Code}'!";
                            _logger.LogError(errorMessage);
                            return (returnList, errorMessage);
                        }

                        foreach (var card in productContentsAgain)
                        {
                            // get inventory that matches opened items
                            var existingInventoryMatchingOpenedItemsAgain = userInventory.FirstOrDefault(x => x.ProductCode == productContentsAgain.First().Product.Code);

                            productsToAddToUserInventory.Add( new Inventory
                            {
                                // add existing count to newly added product count
                                Count = card.Count + (existingInventoryMatchingOpenedItemsAgain?.Count ?? 0),
                                ProductCode = card.Product.Code,
                                UserId = userId,
                                SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)card.Product).SetCode)
                            });

                            // maintain separate list for return to consumer (which doesn't include counts of matching inventory) 
                            uncommittedReturnList.Add(new Inventory
                            {
                                Count = card.Count,
                                ProductCode = card.Product.Code,
                                UserId = userId,
                                SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)card.Product).SetCode)
                            });
                        }
                    }
                }
            }

            // add new items to user inventory
            // remove requested items from inventory
            _logger.LogInformation($"Total Return Count = {productsToAddToUserInventory.Count}");
            var successfulInsert = await _inventoryRepository.UpsertMultipleInventory(new List<List<Inventory>> { productsToAddToUserInventory, existingUserInventoryToUpdate });

            if (!successfulInsert)
            {
                errorMessage = $"database insert was unsuccessful!";
                _logger.LogError(errorMessage);
            }
            else
            {
                returnList = InventoryItemsFromInventory(uncommittedReturnList).AsList();

                await _inventoryRepository.RemoveEmptyUserInventory(userId);
            }

            return (returnList, errorMessage);
        }
    }
}
