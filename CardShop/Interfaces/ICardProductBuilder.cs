
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
        List<InventoryItemInternal> OpenProduct(Product product, int multiplier);
        Product GetProductByProductType(ProductType productType, CardSetCode cardSetCode);
        bool TestCardSetRarityPool(CardSetCode cardSetCode, List<string> rarityCodes, int testCount, bool peekDontDraw);
        List<Product> GetAllExistingProducts();
        List<Card> GetAllExistingCards(string? cardSetName = null);
        List<CardSetInfo> GetAllAvailableCardSetInfo();
        Card? GetCardByName(string cardName);
    }
}
