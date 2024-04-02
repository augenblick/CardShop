namespace CardShop.Models.Request
{
    public class AddSingleCardToDeckRequest
    {
        public int DeckId { get; set; }
        public string CardProductCode { get; set; }
    }
}
