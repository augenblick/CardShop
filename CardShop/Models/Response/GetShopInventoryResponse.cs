using CardShop.Repositories.Models;

namespace CardShop.Models.Response
{
    public class GetShopInventoryResponse : BaseResponse
    {
        public List<InventoryItemInternal> Inventory { get; set; }
    }
}
