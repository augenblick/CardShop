﻿using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IInventoryManager
    {
        Task<bool> AddInventory(List<KeyValuePair<Product, int>> productsToAdd, int userId);
        Task<bool> UpdateMultipleInventory(List<List<Inventory>> inventoryRequests);
        Task<List<Inventory>> GetUserInventory(int userId);
        Task<(List<InventoryItem>, string)> OpenInventoryProducts(int userId, List<ProductReference> itemsToOpen);
        List<InventoryItem> InventoryItemsFromInventory(List<Inventory> inventory);
        bool ClearUserInventory(int userId);
    }
}
