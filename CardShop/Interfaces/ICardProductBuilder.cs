
using CardShop.Enums;
using CardShop.Models;

namespace CardShop.Interfaces
{
    public interface ICardProductBuilder
    {
        Task InitializeCardSets();
        List<object> OpenPremiereProduct(Product product);
        List<Product> GetProducts(string productCode, CardSetCode cardSetCode = CardSetCode.undefined, int count = 1);
        List<Product> GetProducts(ProductType productType, CardSetCode cardSetCode, int count = 1);
        bool TestCardSetRarityPool(CardSetCode cardSetCode, Enums.RarityCode rarityCode, int testCount, bool peekDontDraw);
    }
}
