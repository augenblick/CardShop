using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using System.Collections.Generic;

namespace CardShop.Logic
{
    

    public class InventoryManager : IInventoryManager
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryManager(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            return await _inventoryRepository.GetUserInventory(userId);
        }

        public async Task<List<Inventory>> AddInventory(List<KeyValuePair<Product, int>> productsToAdd, int userId)
        {
            if (productsToAdd == null || productsToAdd.Count < 1)
            {
                // TODO: log error
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
    }
}
