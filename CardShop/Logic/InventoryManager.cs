using CardShop.Extensions;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;
using Dapper;

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

        public async Task<List<InventoryItem>> GetUserInventoryItems(int userId, ProductType? productTypeFilter = null)
        {
            var items = InventoryItemsFromInventory(await GetUserInventory(userId));

            if (productTypeFilter.HasValue)
            {
                return items.Where(x => x.Product.ProductType == productTypeFilter.Value).AsList();
            }

            return items;
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

            if ( itemsToOpen == null || itemsToOpen.Count < 1 || itemsToOpen.Sum(x => x.Count) < 1)
            {
                errorMessage = $"The request list is empty!";
                _logger.LogError(errorMessage);
                return (returnList, errorMessage);
            }

            // group like items
            var groupedRequestItems = itemsToOpen
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

            foreach(var item in groupedRequestItems)
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

            foreach(var matchingItem in inventoryMatchingItemsToOpen)
            {
                var existingInventoryCount = matchingItem.Count;

                var requestedCount = groupedRequestItems.First(x => x.ProductCode == matchingItem.ProductCode).Count;

                if (requestedCount < 1) { continue; }

                // save record to modify existing inventory count at the end
                existingUserInventoryToUpdate.Add(new Inventory
                {
                    ProductCode = matchingItem.ProductCode,
                    SetCode = matchingItem.SetCode,
                    UserId = userId,
                    Count = existingInventoryCount - requestedCount
                });

                var product = _cardProductBuilder.GetProduct(matchingItem.ProductCode);

                if (product == null)
                {
                    errorMessage = $"Product '{matchingItem.ProductCode}' could not be acquired!";
                    _logger.LogError(errorMessage);
                    return (returnList, errorMessage);
                }

                var productContents = new List<InventoryItem>();
                if (product.Contents == null || !product.Contents.Any())
                {
                    // TODO: no contents
                    errorMessage = $"Product {product.Code} has no contents.";
                    _logger.LogError(errorMessage);
                    return (returnList, errorMessage);
                }
                else 
                {
                    productContents = _cardProductBuilder.OpenProduct(product, requestedCount);

                    if (productContents == null)
                    {
                        // TODO:
                        errorMessage = $"Product '{matchingItem.ProductCode}' could not be acquired!";
                        _logger.LogError(errorMessage);
                        return (returnList, errorMessage);
                    }
                }


                foreach (var constituent in productContents.Consolidate())
                {
                    // get inventory that matches opened items
                    var existingInventoryMatchingOpenedItems = userInventory.FirstOrDefault(x => x.ProductCode == constituent.Product.Code);

                    // TODO: make card setcode an enum too
                    var constituentSetCode = constituent.Product is Card ? Enums.CardSetHelpers.GetCardSetCode(((Card)constituent.Product).SetCode) : constituent.Product.SetCode;

                    productsToAddToUserInventory.Add(new Inventory
                    {
                        // add existing count to newly added product count
                        Count = constituent.Count + (existingInventoryMatchingOpenedItems?.Count ?? 0),
                        ProductCode = constituent.Product.Code,
                        UserId = userId,
                        SetCode = constituentSetCode
                    });

                    // maintain separate list for return to consumer (which doesn't include counts of matching inventory) 
                    uncommittedReturnList.Add(new Inventory
                    {
                        Count = constituent.Count,
                        ProductCode = constituent.Product.Code,
                        UserId = userId,
                        SetCode = constituentSetCode
                    });
                }

                // foreach (var card in productContentsAgain)
                //        {
                //            // get inventory that matches opened items
                //            var existingInventoryMatchingOpenedItemsAgain = userInventory.FirstOrDefault(x => x.ProductCode == card.Product.Code);

                    //            productsToAddToUserInventory.Add( new Inventory
                    //            {
                    //                // add existing count to newly added product count
                    //                Count = card.Count + (existingInventoryMatchingOpenedItemsAgain?.Count ?? 0),
                    //                ProductCode = card.Product.Code,
                    //                UserId = userId,
                    //                SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)card.Product).SetCode)
                    //            });

                    //            // maintain separate list for return to consumer (which doesn't include counts of matching inventory) 
                    //            uncommittedReturnList.Add(new Inventory
                    //            {
                    //                Count = card.Count,
                    //                ProductCode = card.Product.Code,
                    //                UserId = userId,
                    //                SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)card.Product).SetCode)
                    //            });
                    //        }



                    //if (product is not BoosterPack)
                    //{
                    //    var productContents = _cardProductBuilder.OpenProduct(product);

                    //    if (productContents == null || productContents.FirstOrDefault() == null)
                    //    {
                    //        errorMessage = $"Couldn't open product '{product.Code}'!";
                    //        _logger.LogError(errorMessage);
                    //        return (returnList, errorMessage);
                    //    }

                    //    foreach (var constituent in productContents)
                    //    {
                    //        // get inventory that matches opened items
                    //        var existingInventoryMatchingOpenedItems = userInventory.FirstOrDefault(x => x.ProductCode == constituent.Product.Code);

                    //        // TODO: make card setcode an enum too
                    //        var constituentSetCode = constituent.Product is Card ? Enums.CardSetHelpers.GetCardSetCode(((Card)constituent.Product).SetCode) : constituent.Product.SetCode;

                    //        productsToAddToUserInventory.Add(new Inventory
                    //        {
                    //            // add existing count to newly added product count
                    //            Count = (constituent.Count * requestedCount) + (existingInventoryMatchingOpenedItems?.Count ?? 0),
                    //            ProductCode = constituent.Product.Code,
                    //            UserId = userId,
                    //            SetCode = constituentSetCode
                    //        });

                    //        // maintain separate list for return to consumer (which doesn't include counts of matching inventory) 
                    //        uncommittedReturnList.Add(new Inventory
                    //        {
                    //            Count = (constituent.Count * requestedCount),
                    //            ProductCode = constituent.Product.Code,
                    //            UserId = userId,
                    //            SetCode = constituentSetCode
                    //        });
                    //    }
                    //}
                    //else
                    //{
                    //    // random draw for each pack
                    //    for (int i = 0; i < requestedCount; i++)
                    //    {
                    //         var productContentsAgain = _cardProductBuilder.OpenProduct(product);

                    //        if (productContentsAgain == null || productContentsAgain.FirstOrDefault() == null)
                    //        {
                    //            errorMessage = $"Couldn't open product '{product.Code}'!";
                    //            _logger.LogError(errorMessage);
                    //            return (returnList, errorMessage);
                    //        }

                    //        foreach (var card in productContentsAgain)
                    //        {
                    //            // get inventory that matches opened items
                    //            var existingInventoryMatchingOpenedItemsAgain = userInventory.FirstOrDefault(x => x.ProductCode == card.Product.Code);

                    //            productsToAddToUserInventory.Add( new Inventory
                    //            {
                    //                // add existing count to newly added product count
                    //                Count = card.Count + (existingInventoryMatchingOpenedItemsAgain?.Count ?? 0),
                    //                ProductCode = card.Product.Code,
                    //                UserId = userId,
                    //                SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)card.Product).SetCode)
                    //            });

                    //            // maintain separate list for return to consumer (which doesn't include counts of matching inventory) 
                    //            uncommittedReturnList.Add(new Inventory
                    //            {
                    //                Count = card.Count,
                    //                ProductCode = card.Product.Code,
                    //                UserId = userId,
                    //                SetCode = Enums.CardSetHelpers.GetCardSetCode(((Card)card.Product).SetCode)
                    //            });
                    //        }
                    //    }
                    //}
            }

            // add new items to user inventory
            // remove opened items from inventory
            _logger.LogInformation($"Total Return Count = {productsToAddToUserInventory.Count}");
            var successfulInsert = await _inventoryRepository.UpsertMultipleInventory(new List<List<Inventory>> { productsToAddToUserInventory, existingUserInventoryToUpdate });

            if (!successfulInsert)
            {
                errorMessage = $"database insert was unsuccessful!";
                _logger.LogError(errorMessage);
            }
            else
            {
                returnList = InventoryItemsFromInventory(uncommittedReturnList).AsList().Consolidate();

                await _inventoryRepository.RemoveEmptyUserInventory(userId);
            }

            return (returnList, errorMessage);
        }
    }
}
