namespace CardShop.Models.Response
{
    public class OpenInventoryProductsResponse : BaseResponse
    {
        public int TotalItems { get; set; }
        public List<InventoryItem> OpenedItems { get; set; }
    }
}
