namespace CardShop.Models.Response
{
    public class PurchaseProductResponse : BaseResponse
    {
        public List<InventoryItem> InventoryItems {  get; set; }
    }
}
