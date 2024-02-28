using CardShop.Models;

namespace CardShop.Interfaces
{
    public interface ICardProductBuilder
    {
        IEnumerable<CardPack> GetCardPacks(int count, string cardSetName);

    }
}
