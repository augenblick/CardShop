using CardShop.Models.Request;

namespace CardShop.Models
{
    public class Deck
    {
        public int DeckId { get; set; }
        public string? DeckName { get; set; }
        public int UserId { get; set; }
        public Enums.DeckType DeckType { get; set; } = Enums.DeckType.Deck;
        public bool IsPublic { get; set; }
        public List<DeckContent> CardList { get; set; } = new List<DeckContent>();
    }

    public class DeckContent
    {
        public string CardProductCode { get; set; }
        public int Count { get; set;}
    }
}
