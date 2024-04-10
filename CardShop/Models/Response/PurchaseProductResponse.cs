namespace CardShop.Models.Response
{
    public class PurchaseProductResponse : BaseResponse
    {
        public decimal TotalCost { get; set; }
        public decimal RemainingUserBalance { get; set; }
        public List<InventoryItemInternal> InventoryItems {  get; set; }
    }
}
