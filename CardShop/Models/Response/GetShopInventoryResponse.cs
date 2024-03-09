using CardShop.Repositories.Models;

namespace CardShop.Models.Response
{
    public class GetShopInventoryResponse : BaseResponse
    {
        public List<Inventory> Inventory { get; set; }
    }
}
