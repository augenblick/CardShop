namespace CardShop.Models.Response
{
    public class GetAllAvailableProductInfoResponse
    {
        public List<Product> Products { get; set; }
        public List<Card> Cards { get; set; }
    }
}
