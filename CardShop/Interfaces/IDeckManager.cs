using CardShop.Models;

namespace CardShop.Interfaces
{
    public interface IDeckManager
    {
        Task<Deck> GetDeck(int deckId);
        Task<Deck> GetDeckNoContents(int deckId);
        Task<bool> DeleteDeck(int deckId, int userId);
        Task<(List<DeckContent>, string)> AddCardsToDeck(int deckId, int userId, List<DeckContent> cardsToAdd);
        Task<(List<DeckContent>, string)> RemoveCardsFromDeck(int deckId, int userId, List<DeckContent> cardsToRemove);
        Task<List<Deck>> GetDecks(int? userId = null, bool? isPublic = null);
    }
}
