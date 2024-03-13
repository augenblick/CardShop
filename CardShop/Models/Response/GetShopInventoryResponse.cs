using CardShop.Repositories.Models;

namespace CardShop.Models.Response
{
    public class GetShopInventoryResponse : BaseResponse
    {
        public List<InventoryItem> Inventory { get; set; }
    }
}
