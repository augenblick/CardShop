using CardShop.Models;

namespace CardShop.Interfaces
{
    public interface IDeckManager
    {
        Task<Deck> GetDeck(int deckId);
        Task<Deck> GetDeckNoContents(int deckId);
        Task<bool> DeleteDeck(int deckId, int userId);
        Task<List<DeckContent>> AddCardsToDeck(int deckId, int userId, List<DeckContent> cardsToAdd);
    }
}
