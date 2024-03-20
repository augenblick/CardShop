using CardShop.Constants;
using Dapper;
using Newtonsoft.Json;

namespace CardShop.Models
{
    public class CardSet
    {
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string SetCode { get; set; }

        [JsonProperty("cycle_code", NullValueHandling = NullValueHandling.Ignore)]
        public string CycleCode { get; set; }

        [JsonProperty("date_release", NullValueHandling = NullValueHandling.Ignore)]
        public string ReleaseDate { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int Size { get; set; }

        [JsonProperty("cards", NullValueHandling = NullValueHandling.Ignore)]
        public List<Card> Cards { get; set; }

        [JsonProperty("products", NullValueHandling = NullValueHandling.Ignore)]
        public List<Product> Products { get; set; }

        public List<CardRarity> CardRarityMap { get; set; }

        private List<CardPool> _cardRarityPools { get; set; } = new List<CardPool>();

        public async Task Initialize()
        {
                foreach(var cardRarity in CardRarityMap)
                {
                    if (!_cardRarityPools.Any(x => x.PoolRarityCode.ToString() == cardRarity.OverallRarity))
                    {
                        _cardRarityPools.Add(new CardPool(cardRarity.OverallRarity));
                    }
                }

                foreach (var card in Cards)
                {
                    var cardRarity = CardRarityMap.FirstOrDefault(x => x.Rarity == card.RarityCode);
                    var cardCount = cardRarity?.Count;
                    var cardOverallRarity = cardRarity?.OverallRarity;

                    var cardPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode.ToString() == cardOverallRarity);

                    if (cardPool == null || cardCount < 1)
                    {
                        StaticHelpers.Logger.LogError($"When trying to insert cards into a card pool for set '{SetCode}', the card pool with overallRarity '{cardOverallRarity}' could not be found!");
                    }

                    cardPool?.AddCard(card, cardCount.GetValueOrDefault());
                }
        }

        public Card DrawRandomCardFromSet(string rarityCode, bool peekDontDraw = true)
        {
            var cardPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == rarityCode);

            if (cardPool == null)
            {
                StaticHelpers.Logger.LogError($"No card pool in set '{Name}' with rarity code '{rarityCode}'");
                return null;
            }

            if (peekDontDraw)
            {
                return cardPool.PeekCard();
            }

            return cardPool.DrawCard();
        }

        public List<InventoryItem> OpenProduct(Product product)
        {
            var returnProductList = new List<InventoryItem>();

            if (product is BoosterPack)
            {
                return OpenBoosterPack((BoosterPack)product);
            }
            foreach (var content in ((BoosterBox)product).Contents)
            {
                var selectedContent = Products.FirstOrDefault(x => x.Code == content.Code);

                if (selectedContent != null) 
                { 
                    returnProductList.Add(new InventoryItem
                    {
                        Product = selectedContent,
                        Count = content.Count
                    }); 
                }
                else
                {
                    StaticHelpers.Logger.LogError($"No matching product '{product.Code}' found in set '{SetCode}' while opening products!");
                }
            }

            var consolidatedList = returnProductList
                .GroupBy(x => x.Product.Code)
                .Select(y => new InventoryItem
                {
                    Product = y.First().Product,
                    Count = y.Sum(c => c.Count)
                }).AsList();

            return consolidatedList;
        }

        public Product GetCardSetProduct(string productCode)
        {
            var product = Products.FirstOrDefault(x => x.Code == productCode) ?? Cards.FirstOrDefault(x => x.Code == productCode);

            return product;
        }

        public List<InventoryItem> OpenBoosterPack(BoosterPack pack)
        {
            var drawnCards = new List<Product>();

            foreach (var raritySpec in pack.PackContentSpecs)
            {
                List<Card> chosenCards = new List<Card>();

                var count = raritySpec.Count;
                var overallRarityForDraw = raritySpec.OverallRarity;

                // TODO: update PoolRarityCode options to be defined dynamically based on cardset definition jsons
                var cardPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == overallRarityForDraw);

                if (cardPool == null)
                {
                    StaticHelpers.Logger.LogError($"A cardpool with rarity '{overallRarityForDraw}' was not found within cardset '{SetCode}'");
                    break;
                }

                chosenCards = cardPool.PeekCards(count);

                if (chosenCards.Count < count)
                {
                    StaticHelpers.Logger.LogError($"Unable to draw enough cards while opening a booster pack!  OverallRarity: '{overallRarityForDraw}', Count: '{count}'");
                }

                drawnCards.AddRange(chosenCards);
            }

            if (drawnCards.Count < 1 || drawnCards.Any(x => string.IsNullOrWhiteSpace(x.Code)))
            {
                StaticHelpers.Logger.LogError("Test");
            }

            var returnList = drawnCards.GroupBy(p => p.Code)
                    .Select(group => new InventoryItem
                    {
                        Product = group.First(),
                        Count = group.Count()
                    })
                    .ToList();
            
            return returnList;
        }

        private int GetSetCardCountByRarity(string rarity)
        {
            switch (Name)
            {
                case CardSetConstants.Premiere:
                    switch (rarity)
                    {

                        case "C1": return 1;
                        case "C2": return 2;
                        case "C3": return 3;

                        case "U1": return 2;
                        case "U2": return 4;

                        case "R1": return 2;
                        case "R2": return 4;
                    }
                    return -1;
                case CardSetConstants.NewHope:
                    switch (rarity)
                    {

                        case "C1": return 1;
                        case "C2": return 2;
                        case "C3": return 3;
                        case "U1": return 1;
                        case "U2": return 2;
                        case "R1": return 1;
                        case "R2": return 2;
                    }
                    return -1;
                case CardSetConstants.Hoth:
                    switch (rarity)
                    {

                        case "C1": return 1;
                        case "C2": return 2;
                        case "C3": return 3;
                        case "U1": return 1;
                        case "U2": return 2;
                        case "R1": return 1;
                        case "R2": return 2;
                    }
                    return -1;
                case CardSetConstants.Dagobah:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                    }
                    return -1;
                case CardSetConstants.CloudCity:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                    }
                    return -1;
                case CardSetConstants.JabbasPalace:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                    }
                    return -1;
                case CardSetConstants.SpecialEdition:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Endor:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                        case "CF": return 9;
                        case "UF": return 4;
                        case "RF": return 2;
                    }
                    return -1;
                case CardSetConstants.DeathStar2:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "UR": return 2;
                        case "R3": return 3;
                        case "R4": return 4;
                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Tatooine:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R1": return 1;
                        case "R3": return 3;
                        case "R4": return 4;
                    }
                    return -1;
                case CardSetConstants.Coruscant:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R1": return 1;
                        case "R3": return 3;
                        case "R4": return 4;
                    }
                    return -1;
                case CardSetConstants.Theed:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R1": return 1;
                        case "R3": return 3;
                        case "R4": return 4;
                    }
                    return -1;
                case CardSetConstants.Reflections1:
                    switch (rarity)
                    {

                        case "CF": return 3;
                        case "UF": return 2;
                        case "RF": return 1;
                    }
                    return -1;
                case CardSetConstants.Reflections2:
                    switch (rarity)
                    {

                        case "CF": return 3;
                        case "UF": return 2;
                        case "RF": return 1;
                        case "F1": return 1;
                        case "F2": return 1;
                    }
                    return -1;
                case CardSetConstants.Reflections3:
                    switch (rarity)
                    {

                        case "CF": return 3;
                        case "UF": return 2;
                        case "RF": return 1;
                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.PremiereEnhanced:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.CloudCityEnhanced:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.JabbasPalaceEnhanced:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.JediPack:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.ThirdAnthology:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.PremiereSealedDeck:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.JabbasPalaceSealedDeck:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Promotional:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Starter1:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Starter2:
                    switch (rarity)
                    {
                        case "F": return 1;
                    }
                    return -1;
                default:
                    return -1;
            }
        }
    }

    public class CardRarity
    {
        public string Rarity { get; set; }
        public int Count { get; set; }
        public string OverallRarity { get; set; }
    }
}
