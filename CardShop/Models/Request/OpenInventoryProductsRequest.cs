namespace CardShop.Models.Request
{
    public class OpenInventoryProductsRequest
    {
        public int UserId { get; set; }
        public List<ProductReference> InventoryProductsToOpen { get; set; }
    }
}
