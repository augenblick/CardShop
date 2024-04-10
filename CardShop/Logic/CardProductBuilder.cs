using CardShop.Enums;
using CardShop.Extensions;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace CardShop.Logic
{
    // TODO: a lot of room for optimization throughout

    public class CardProductBuilder : ICardProductBuilder
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private List<CardSet> _cardSets;

        private readonly Random _randomizer = new Random();
        private readonly ILogger _logger;
        private List<Product> _allExistingProducts = new List<Product>();
        private List<Card> _allExistingCards = new List<Card>();

        public CardProductBuilder(ILogger<CardProductBuilder> logger)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
                {
                    Converters = { new ProductConverter() }
                };

            _logger = logger;
        }

        public List<CardSetCode> GetAvailableCardSets(List<string> cycleCodes)
        {
            var availableSets = _cardSets.Where(x => cycleCodes.Contains(x.CycleCode)).Select(y => CardSetHelpers.GetCardSetCode(y.SetCode)).ToList();

            return availableSets;
        }
        
        public Product GetProduct(Inventory inventory)
        {
            return GetProduct(inventory.ProductCode, inventory.SetCode);
        }

        public List<InventoryItem> OpenProductOld(Product product)
        {
            var cardSet = GetCardSetByCardSetCode(product.SetCode);
            var contents = cardSet.OpenProduct(product);

            return contents;
        }

        public List<InventoryItem> OpenProduct(Product product, int multiplier)
        {
            if (product == null)
            {
                return new List<InventoryItem>();
            }

            var returnProductList = new List<InventoryItem>();

            foreach(var content in product.Contents)
            {
                CardSet sourceCardSet;
                content.Count *= multiplier;

                // TODO: this cardsetcode stuff is a mess and needs to be cleaned up
                if (content.SetCodes != null && content.SetCodes.Count > 0)
                {
                    var chosenSetCode = content.SetCodes.First();
                    if (content.SetCodes.Count > 1)
                    {
                        // choose a setcode at random from those provided
                        var setCodeIndex = _randomizer.Next(content.SetCodes.Count);
                        chosenSetCode = content.SetCodes[setCodeIndex];
                    }

                    sourceCardSet = GetCardSetByCardSetCode(CardSetHelpers.GetCardSetCode(chosenSetCode));
                }
                else
                {
                    sourceCardSet = GetCardSetByCardSetCode(product.SetCode);
                }

                if (sourceCardSet == null)
                {
                    // TODO: we have a problem
                    _logger.LogError($"No source set found for this content");
                }

                if (content.RandomPickParameters != null)
                {
                    // make random pick
                    returnProductList.AddRange(sourceCardSet.MakeRandomPicks(content));
                }
                else
                {
                    returnProductList.Add(sourceCardSet.OpenProduct(content));
                }
            }

            

            var consolidatedList = returnProductList.Consolidate();

            return consolidatedList;
        }

        public List<CardSetInfo> GetAllAvailableCardSetInfo()
        {
            return _cardSets.Select(x => new CardSetInfo
            {
                Name = x.Name,
                SetCode = x.SetCode,
                CycleCode = x.CycleCode,
                Position = x.Position,
                ReleaseDate = x.ReleaseDate,
                Size = x.Size
            }).AsList();
        }

        public Product GetProduct(string productCode, CardSetCode cardSetCode = CardSetCode.undefined)
        {
            var returnProduct = new Product();

            var cardSet = GetCardSetByCardSetCode(cardSetCode) ?? _cardSets.FirstOrDefault(x => x.Products.Any(y => y.Code == productCode)) ?? _cardSets.FirstOrDefault(x => x.Cards.Any(y => y.Code == productCode));

            if (cardSet == null) 
            {
                _logger.LogError($"Product '{productCode}' not found in any available card set!");
                return returnProduct; 
            }

            var product = cardSet.Products.FirstOrDefault(x => x.Code == productCode) ?? cardSet.Cards.FirstOrDefault(x => x.Code == productCode);

            if (product != null)
            {
                // TODO: cost and setcode will eventually be defined per product within each set.json
                var perPackCost = 2.50M;

                product.SetCode = product.SetCode == CardSetCode.undefined ? Enums.CardSetHelpers.GetCardSetCode(cardSet.SetCode) : product.SetCode;

                if (product is BoosterBox)
                {
                    if (product.CostPer == 0M)
                    {
                        var cost = ((BoosterBox)product).Contents.FirstOrDefault()?.Count * perPackCost * 0.75M;
                        product.CostPer = cost ?? 0.00M;
                    }
                }
                else if (product is BoosterPack)
                {
                    if (product.CostPer == 0M)
                    {
                        product.CostPer = perPackCost;
                    }
                }

                returnProduct = product;
            }
            else
            {
                _logger.LogError($"Product '{productCode}' could not be found in set '{cardSet.SetCode}'");
            }

            return returnProduct;
        }

        public Product GetProductByProductType(ProductType productType, CardSetCode cardSetCode)
        {
            var returnProduct = new Product();

            var cardSet = GetCardSetByCardSetCode(cardSetCode);

            if (cardSet == null) { return returnProduct; }

            var products = cardSet.Products.Where(x => x.ProductType == productType).AsList();

            if (products.Count < 1) { return returnProduct; }
            var randomIndex = _randomizer.Next(products.Count);
            var product = products[randomIndex];

            if (product != null)
            {
                // TODO: cost and setcode will eventually be defined per product within each set.json
                var perPackCost = 2.50M;

                product.SetCode = product.SetCode == CardSetCode.undefined ? cardSetCode : product.SetCode;

                if (product is BoosterBox)
                {
                    if (product.CostPer == 0M)
                    {
                        var cost = ((BoosterBox)product).Contents.FirstOrDefault()?.Count * perPackCost * 0.75M;
                        product.CostPer = cost ?? 0.00M;
                    }
                }
                else if (product is BoosterPack)
                {
                    if (product.CostPer == 0M)
                    {
                        product.CostPer = perPackCost;
                    }
                }
                else if (product is StarterDeck)
                {
                    if (product.CostPer == 0M)
                    {
                        product.CostPer = perPackCost * 3.5M;
                    }
                }

                returnProduct = product;
            }

            return returnProduct;
        } 

        private CardSet GetCardSetByCardSetCode(CardSetCode cardSetCode)
        {
            var code = cardSetCode.GetCardSetCodeString();
            return _cardSets.FirstOrDefault(x => x.SetCode == cardSetCode.GetCardSetCodeString());
        }

        public bool TestCardSetRarityPool(CardSetCode cardSetCode, List<string> rarityCodes, int testCount, bool peekDontDraw)
        {
            //var cardSetCodeName = cardSetCode.GetCardSetCodeString();
            var cardSet = GetCardSetByCardSetCode(cardSetCode);
            try
            {
                // check null
                var cardPool = new CardPool("F");

                for (int i = 0; i < testCount; i++)
                {
                    cardPool.AddCardNoDuplicates(cardSet.DrawRandomCardFromSet(rarityCodes, null, peekDontDraw), 1);
                }

                var poolStats = cardPool.GetPoolStatistics();
                _logger.LogInformation($"Total entries into pool: {cardPool.TotalCardCount}");

                poolStats.PrintStatistics();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Exception while drawing card into new pool for statistics check.", new[] { new { CardSet = cardSet, RarityCodes = rarityCodes } });
                return false;
            }

            return true;
        }

        public async Task InitializeCardSets()
        {
            var initTaskList = new List<Task>();
            
            try
            {
                _cardSets = IngestJsonData<CardSet>("Repositories/Data/CardSets");

                foreach(var set in _cardSets.Where(x => x.CycleCode == "full" || x.CycleCode == "premium"))
                {
                    var emptyContentProducts = set.Products.Where(x => x.Contents == null || x.Contents.Count < 1);
                    if (emptyContentProducts.Any())
                    {
                        _logger.LogError($">>>>>>>>>>>>>>>>>>>>>> set '{set.Name}'!");
                        foreach(var emptyProduct in emptyContentProducts)
                        {
                            _logger.LogError($":::::: product '{emptyProduct.Name}'");
                        }
                    }
                    initTaskList.Add(set.Initialize());
                }

                await Task.WhenAll(initTaskList);

                foreach(var cardSet in _cardSets)
                {
                    var emptyContentProducts = cardSet.Products.Where(x => x.Contents == null || x.Contents.Count < 1);
                    if (emptyContentProducts.Any())
                    {
                        _logger.LogError($">>>>***************>>>>> set '{cardSet.Name}'!");
                        foreach (var emptyProduct in emptyContentProducts)
                        {
                            _logger.LogError($":::********::: product '{emptyProduct.Name}'");
                        }
                    }
                }

                BuildAllExistingProducts();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Exception while initializing the card sets.");
            }
        }

        public List<Product> GetAllExistingProducts()
        {
            return _allExistingProducts;
        }

        public List<Card> GetAllExistingCards(string cardSetName = null)
        {
            if (string.IsNullOrWhiteSpace(cardSetName))
            {
                return GetExistingCardsFromSet(cardSetName);
            }

            return _allExistingCards;
        }

        public Card? GetCardByName(string cardName)
        {
            Card? returnCard = null;

            foreach (var cardSet in _cardSets)
            {
                var card = cardSet.GetCardByName(cardName);

                if (card != null )
                {
                    if (returnCard != null)
                    {
                        // duplicate match found!
                        return null;
                    }

                    returnCard = card;
                }
            }

            return returnCard;
        }

        private List<Card> GetExistingCardsFromSet(string cardSetName)
        {
            var cardSet = _cardSets.FirstOrDefault(x => x.SetCode == cardSetName);

            if (cardSet == null) { return new List<Card>(); }

            return cardSet.Cards;
        }

        private void BuildAllExistingProducts()
        {
            var existingProducts = new List<Product>();
            var existingCards = new List<Card>();

            foreach (var set in _cardSets.OrderBy(x => x.Position))
            {
                existingProducts.AddRange(set.Products.OrderBy(x => x.ProductType));
                existingCards.AddRange(set.Cards.OrderBy(x => x.Code));
            }

            _allExistingProducts = existingProducts.AsList();
            _allExistingCards = existingCards.AsList();
        }

        private List<T> IngestJsonData<T>(string jsonDirectoryPath)
        {
            
            var allObjects = new List<T>();
            var currentFileName = string.Empty;

            try
            {
                var files = Directory.GetFiles(jsonDirectoryPath);
                

                foreach (var file in files)
                {
                    if (file.Contains(".json"))
                    {
                        currentFileName = file;
                        using StreamReader reader = new StreamReader(file);
                        var json = reader.ReadToEnd();
                        var obj = JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);

                        if (obj != null)
                        {
                            allObjects.Add(obj);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred while ingesting file '{currentFileName}'.");
            }

            return allObjects;
        }

        private class ProductConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Product);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jsonObject = JObject.Load(reader);
                string productType = jsonObject["productType"].ToObject<string>();

                switch (productType)
                {
                    case "BoosterPack":
                        return jsonObject.ToObject<BoosterPack>(serializer);
                    case "BoosterBox":
                        return jsonObject.ToObject<BoosterBox>(serializer);
                    case "StarterDeck":
                        return jsonObject.ToObject<StarterDeck>(serializer);
                    case "EnhancedPack":
                        return jsonObject.ToObject<EnhancedPack>(serializer);
                    case "PromotionalSet":
                        return jsonObject.ToObject<PromotionalSet>(serializer);
                    case "SealedDeck":
                        return jsonObject.ToObject<SealedDeck>(serializer);
                    case "StarterSet":
                        return jsonObject.ToObject<StarterSet>(serializer);
                    case "AnthologySet":
                        return jsonObject.ToObject<AnthologySet>(serializer);
                    default:
                        throw new JsonSerializationException($"Unknown product type: {productType}");
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
