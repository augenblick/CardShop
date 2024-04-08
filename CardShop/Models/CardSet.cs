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
        public List<Card> Cards { get; set; } = new List<Card>();

        [JsonProperty("products", NullValueHandling = NullValueHandling.Ignore)]
        public List<Product> Products { get; set; }

        public List<CardRarity> CardRarityMap { get; set; } = new List<CardRarity>();

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
                        //StaticHelpers.Logger.LogError($"When trying to insert cards into a card pool for set '{SetCode}', the card pool with overallRarity '{cardOverallRarity}' could not be found!");
                    }

                    cardPool?.AddCard(card, cardCount.GetValueOrDefault());
                }
        }

        public Card DrawRandomCardFromSet(List<string> overallRarityCodes, string? cardSide = null, bool peekDontDraw = true)
        {
            CardPool? cardPool = null;

            if (overallRarityCodes.Count == 1)
            {
                cardPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == overallRarityCodes.First());
            }
            else
            {
                // create unique identifier for this pool
                overallRarityCodes.Sort();
                var uniqueCombinedName = string.Join('-', overallRarityCodes.Select(x => x.ToUpper()));

                // see if such a pool has already been generated
                cardPool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == uniqueCombinedName);
                if ( cardPool == null)
                {
                    var suitableCardPools = new List<CardPool>();

                    foreach(var poolRequest in overallRarityCodes)
                    {
                        var multiplier = 1;
                        var request = poolRequest;
                        if (poolRequest.Contains(':'))
                        {
                            // pool definition includes multiplier
                            var split = poolRequest.Split(':');
                            if (!int.TryParse(split[0], out multiplier))
                            {
                                StaticHelpers.Logger.LogError($"Unable to parse the pool request '{poolRequest}'");
                                continue;
                            }

                            request = split[1];
                        }

                        var suitablePool = _cardRarityPools.FirstOrDefault(x => x.PoolRarityCode == request);
                        if (suitablePool == null) 
                        { 
                            StaticHelpers.Logger.LogError($"Unable to find the pool '{request}' in set '{Name}'");
                            continue;
                        }

                        if (multiplier > 1)
                        {
                            suitablePool.ApplyMultiplier(multiplier);
                        }

                        suitableCardPools.Add(suitablePool);
                    }

                    if (suitableCardPools.Count < overallRarityCodes.Count)
                    {
                        StaticHelpers.Logger.LogError($"Not all of requested rarity codes '{overallRarityCodes.Select(x => $"{x}, ")}' found in cardset '{Name}'");
                        return null;
                    }

                    cardPool = new CardPool(suitableCardPools, uniqueCombinedName);
                }
            }

            if (cardPool == null)
            {
                StaticHelpers.Logger.LogError($"Not all of requested rarity codes '{overallRarityCodes.Select(x => $"{x}, ")}' found in cardset '{Name}'");
                return null;
            }

            // save for later use
            _cardRarityPools.Add(cardPool);

            if (peekDontDraw)
            {
                return cardPool.PeekCard(cardSide);
            }

            return cardPool.DrawCard(cardSide);
        }

        public List<InventoryItem> OpenProduct(Product product)
        {
            var returnProductList = new List<InventoryItem>();

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

        public InventoryItem OpenProduct(Content content)
        {
            var returnProduct = new InventoryItem();

            var selectedContent = Products.FirstOrDefault(x => x.Code == content.Code) ?? Cards.FirstOrDefault(x => x.Code == content.Code);

            if (selectedContent != null)
            {
                returnProduct = new InventoryItem
                {
                    Product = selectedContent,
                    Count = content.Count
                };
            }
            else
            {
                StaticHelpers.Logger.LogError($"No matching product '{content.Code}' found in set '{SetCode}' while opening products!");
            }

            return returnProduct;
        }

        public Product GetCardSetProduct(string productCode)
        {
            var product = Products.FirstOrDefault(x => x.Code == productCode) ?? Cards.FirstOrDefault(x => x.Code == productCode);

            return product;
        }

        public List<InventoryItem> MakeRandomPicks(Content content)
        {
            var randomPicks = new List<Product>();

            if (content?.RandomPickParameters?.OverallRarities?.FirstOrDefault() == null)
            {
                return new List<InventoryItem>();
            }

            for (int i = 0; i < content.Count; i++)
            {
                var pick = DrawRandomCardFromSet(content.RandomPickParameters.OverallRarities, content.RandomPickParameters.Side);
                if (pick != null)
                {
                    randomPicks.Add(pick);
                }
            }


            if (randomPicks.Count < 1 || randomPicks.Any(x => string.IsNullOrWhiteSpace(x.Code)))
            {
                StaticHelpers.Logger.LogError("Test");
            }

            return randomPicks.Select(x => new InventoryItem { Count = 1, Product = x}).AsList();
        }

        public Card? GetCardByName(string cardName)
        {
            var foundCards = Cards.Where(x => x.Name?.ToLower() == cardName?.ToLower()).AsList();

            if (foundCards.Count > 1)
            {
                StaticHelpers.Logger.LogError($"Multiple cards with name '{cardName}' found!");
                return null;
            }

            return foundCards.FirstOrDefault();
        }
    }

    public class CardRarity
    {
        public string Rarity { get; set; }
        public int Count { get; set; }
        public string OverallRarity { get; set; }
    }
}
