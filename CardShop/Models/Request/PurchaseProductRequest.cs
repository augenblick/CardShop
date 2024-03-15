namespace CardShop.Models.Request
{
    public class PurchaseProductRequest
    {
        public int PurchaserId { get; set; }
        public List<PurchaseRequest> InventoryItems { get; set; } = [];
    }

    public class PurchaseRequest
    {
        public int InventoryId { get; set; }
        public int Count { get; set; }
    }
}
