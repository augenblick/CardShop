
using CardShop.Enums;
using CardShop.Models;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface ICardProductBuilder
    {
        Task InitializeCardSets();
        List<CardSetCode> GetAvailableCardSets(List<string> cycleCodes);
        Product GetProduct(Inventory inventory);
        Product GetProduct(string productCode, CardSetCode cardSetCode = CardSetCode.undefined);
        List<InventoryItem> OpenProduct(Product product);
        Product GetProductByProductType(ProductType productType, CardSetCode cardSetCode);
        bool TestCardSetRarityPool(CardSetCode cardSetCode, string rarityCode, int testCount, bool peekDontDraw);
        List<Product> GetAllExistingProducts();
    }
}
