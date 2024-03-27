namespace CardShop.Models.Request
{
    public class AddCardsToDeckRequest
    {
        public int DeckId {  get; set; }
        public List<DeckContent> CardsToAdd { get; set; } = new List<DeckContent>();
    }
}
