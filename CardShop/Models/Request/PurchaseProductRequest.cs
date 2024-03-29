namespace CardShop.Models.Request
{
    public class PurchaseProductRequest
    {
        //public int PurchaserId { get; set; }
        public List<ProductReference> InventoryItems { get; set; }
    }

    public class ProductReference
    {
        public string ProductCode { get; set; }
        public int Count { get; set; }
    }
}
