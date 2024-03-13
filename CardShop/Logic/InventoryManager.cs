using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using System.Collections.Generic;

namespace CardShop.Logic
{
    

    public class InventoryManager : IInventoryManager
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly ILogger _logger;

        public InventoryManager(IInventoryRepository inventoryRepository, ICardProductBuilder cardProductBuilder, ILogger<InventoryManager> logger)
        {
            _inventoryRepository = inventoryRepository;
            _cardProductBuilder = cardProductBuilder;
            _logger = logger;
        }

        public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            return await _inventoryRepository.GetUserInventory(userId);
        }

        public async Task<List<Inventory>> AddInventory(List<KeyValuePair<Product, int>> productsToAdd, int userId)
        {
            if (productsToAdd == null || productsToAdd.Count < 1)
            {
                return null;
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

            // TODO: test performance and optimize if necessary
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
                    InventoryId = inventoryItem.InventoryId,
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
    }
}
