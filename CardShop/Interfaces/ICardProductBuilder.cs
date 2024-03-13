
using CardShop.Enums;
using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface ICardProductBuilder
    {
        Task InitializeCardSets();
        List<CardSetCode> GetAvailableCardSets(List<string> cycleCodes);
        List<Product> GetProducts(Inventory inventory);
        List<Product> GetProducts(string productCode, CardSetCode cardSetCode = CardSetCode.undefined, int count = 1);
        List<KeyValuePair<Product, int>> GetProductsByProductType(ProductType productType, CardSetCode cardSetCode, int count = 1);
        bool TestCardSetRarityPool(CardSetCode cardSetCode, Enums.RarityCode rarityCode, int testCount, bool peekDontDraw);
    }
}
