using CardShop.Constants;
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
                    _cardRarityPools.Add(new CardPool((Enums.RarityCode)Enum.Parse(typeof(Enums.RarityCode), cardRarity.OverallRarity)));
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

        public Card DrawRandomCardFromSet(Enums.RarityCode rarityCode, bool peekDontDraw = true)
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

        public List<Product> OpenProduct(Product product)
        {
            var returnProductList = new List<Product>();

            if (product.ProductType == ProductType.BoosterPack)
            {
                return OpenBoosterPack(product);
            }

            foreach (var content in product.Contents)
            {
                for (int i = 0; i < content.Count; i++) 
                {
                    var selectedContent = Products.FirstOrDefault(x => x.Code == content.Code);

                    if (selectedContent != null) 
                    { 
                        returnProductList.Add(selectedContent); 
                    }
                    else
                    {
                        StaticHelpers.Logger.LogError("No matching product found while opening a product!");
                    }
                }
            }

            return returnProductList;
        }

        private List<Product> OpenBoosterPack(Product product)
        {
            var returnCardList = new List<Product>();

            var commonPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == Enums.RarityCode.C);
            var uncommonPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == Enums.RarityCode.U);
            var rarePool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == Enums.RarityCode.R);
            var fixedPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == Enums.RarityCode.F);

            foreach (var content in product.Contents)
            {
                List<Card> chosenCards = new List<Card>();

                switch (content.Code)
                {
                    case "C":
                        chosenCards = commonPool.PeekCards(content.Count);
                        break;
                    case "U":
                        chosenCards = uncommonPool.PeekCards(content.Count);
                        break;
                    case "R":
                        chosenCards = rarePool.PeekCards(content.Count);
                        break;
                    case "F":
                        // TODO: will probably handle fixed cards a different way
                        chosenCards = fixedPool.PeekCards(content.Count);
                        break;
                }

                if (chosenCards.Count < content.Count)
                {
                    StaticHelpers.Logger.LogError($"Unable to draw enough cards while opening a booster pack!  OverallRarity: '{content.Code}', Count: '{content.Count}'");
                }

                returnCardList.AddRange(chosenCards);
            }


            return returnCardList;
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
