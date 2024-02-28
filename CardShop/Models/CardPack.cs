using CardShop.Repositories.Models;

namespace CardShop.Models
{
    public class CardPack
    {
        public string CardSet { get; set; }
        public List<Card> CardList { get; set; } = new List<Card>();
    }
}
