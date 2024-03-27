using CardShop.Models;

namespace CardShop.Interfaces
{
    public interface IDeckRepository
    {
        Task<Deck> CreateDeck(Deck deck);
        Task<Deck> GetDeck(int deckId);
        Task<List<DeckContent>> GetDeckContents(int deckId);
        Task<bool> DeleteDeck(int deckId);
        Task<bool> AddCardsToDeck(int deckId, List<DeckContent> cardsToAdd);
    }
}
