using CardShop.Models;

namespace CardShop.Interfaces
{
    public interface IDeckRepository
    {
        Task<Deck> CreateDeck(Deck deck);
        Task<Deck> GetDeck(int deckId);
        Task<List<DeckContent>> GetDeckContents(int deckId);
        Task<bool> DeleteDeck(int deckId);
        Task<bool> UpsertDeckContents(int deckId, List<DeckContent> cardsToAdd);
        Task<bool> ClearZeroedDeckContents(int deckId);
        Task<List<Deck>> GetDecks(int? userId, bool? isPublic);
    }
}
