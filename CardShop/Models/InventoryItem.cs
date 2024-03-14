namespace CardShop.Models
{
    public class InventoryItem
    {
        public int InventoryId { get; set; }
        public int Count { get; set; }
        public Product Product { get; set; }
        
    }
}
