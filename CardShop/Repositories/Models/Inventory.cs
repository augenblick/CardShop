using CardShop.Models;

namespace CardShop.Repositories.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public Product Product { get; set; }
        public string SetCode { get; set; }
        public int Count { get; set; }
        public int UserId { get; set; }
    }
}
