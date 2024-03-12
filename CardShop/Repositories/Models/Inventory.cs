using CardShop.Models;

namespace CardShop.Repositories.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public string ProductCode { get; set; }
        public Enums.CardSetCode SetCode { get; set; }
        public int Count { get; set; }
        public int UserId { get; set; }
    }
}
