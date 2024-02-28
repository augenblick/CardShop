using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface ICardTestRepository
    {
        Task<Card> GetCardByCardIdAsync(int Id);
        Task<IEnumerable<Card>> GetAllCardsAsync();
        Task<IEnumerable<Card>> GetCardSet(string setName);
        IEnumerable<Card> GetCardSets();
    }
}
