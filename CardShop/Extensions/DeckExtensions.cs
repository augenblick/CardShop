using CardShop.Models;

namespace CardShop.Extensions
{
    public static class DeckExtensions
    {
        public static List<DeckContent> Consolidate(this List<DeckContent> deckContents)
        {
            return deckContents.GroupBy(x => x.CardProductCode)
                                .Select(y => new DeckContent
                                {
                                    CardProductCode = y.First().CardProductCode,
                                    Count = y.Sum(c => c.Count)
                                }).ToList();
        }
    }
}
