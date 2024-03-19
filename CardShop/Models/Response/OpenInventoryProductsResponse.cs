namespace CardShop.Models.Response
{
    public class OpenInventoryProductsResponse : BaseResponse
    {
        public List<InventoryItem> OpenedItems { get; set; }
    }
}
