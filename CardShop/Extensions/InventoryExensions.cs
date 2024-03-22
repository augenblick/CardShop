using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Extensions
{
    public static class InventoryExensions
    {
        /// <summary>
        /// Combines like Inventory by ProductCode and combines their Counts
        /// </summary>
        /// <param name="inventoryList"></param>
        /// <returns></returns>
        public static List<Inventory> Consolidate(this List<Inventory> inventoryList)
        {
            return inventoryList.GroupBy(x => x.ProductCode)
                                .Select(y => new Inventory
                                {
                                    ProductCode = y.First().ProductCode,
                                    SetCode = y.First().SetCode,
                                    UserId = y.First().UserId,
                                    Count = y.Sum(c => c.Count)
                                }).ToList();
        }

        /// <summary>
        /// Combines like InventoryItems by their Product's ProductCode and combines their Counts
        /// </summary>
        /// <param name="inventoryList"></param>
        /// <returns></returns>
        public static List<InventoryItem> Consolidate(this List<InventoryItem> inventoryList)
        {
            return inventoryList.GroupBy(x => x.Product.Code)
                                .Select(y => new InventoryItem
                                {
                                    Product = y.First().Product,
                                    Count = y.Sum(c => c.Count)
                                }).ToList();
        }
    }
}
