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

            //var cardSet = GetCardSetByCardSetCode(product.SetCode);

            //if (product is BoosterPack)
            //{
            //    var packContents = cardSet.OpenBoosterPack(product as BoosterPack);

            //    return packContents;
            //}

            foreach(var content in product.Contents)
            {
                CardSet sourceCardSet;
                content.Count *= multiplier;

                // TODO: this cardsetcode stuff is a mess and needs to be cleaned up
                if (!string.IsNullOrWhiteSpace(content.SetCode))
                {
                    sourceCardSet = GetCardSetByCardSetCode(CardSetHelpers.GetCardSetCode(content.SetCode));
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

            //var contents = new List<Content>();
            //if (product is BoosterBox) { contents = ((BoosterBox)product).Contents; }
            //else if (product is StarterDeck) { contents = ((StarterDeck)product).Contents; }
            //else { _logger.LogError($"ProductType for not '{product.Code}' not supported!"); }

            //foreach (var content in contents)
            //{
            //    var setForThisContent = cardSet;
            //    if (!string.IsNullOrWhiteSpace(content.SetCode))
            //    {
            //        // a specific set is defined for this content, so use it.
            //        setForThisContent = GetCardSetByCardSetCode(CardSetHelpers.GetCardSetCode(content.SetCode));
            //    }

            //    var selectedContent = setForThisContent.GetCardSetProduct(content.Code);

            //    if (selectedContent != null)
            //    {
            //        returnProductList.Add(new InventoryItem
            //        {
            //            Product = selectedContent,
            //            Count = content.Count
            //        });
            //    }
            //    else
            //    {
            //        StaticHelpers.Logger.LogError($"No matching product '{product.Code}' found in set '{setForThisContent.SetCode}' while opening products!");
            //    }
            //}

            // consolidate any like Contents
            //var consolidatedList = returnProductList
            //    .GroupBy(x => x.Product.Code)
            //    .Select(y => new InventoryItem
            //    {
            //        Product = y.First().Product,
            //        Count = y.Sum(c => c.Count)
            //    }).AsList();

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
            return _cardSets.FirstOrDefault(x => x.SetCode == cardSetCode.GetCardSetCodeString());
        }

        public bool TestCardSetRarityPool(CardSetCode cardSetCode, string rarityCode, int testCount, bool peekDontDraw)
        {
            //var cardSetCodeName = cardSetCode.GetCardSetCodeString();
            var cardSet = GetCardSetByCardSetCode(cardSetCode);
            try
            {
                // check null
                var cardPool = new CardPool("F");

                for (int i = 0; i < testCount; i++)
                {
                    cardPool.AddCardNoDuplicates(cardSet.DrawRandomCardFromSet(rarityCode, null, peekDontDraw), 1);
                }

                var poolStats = cardPool.GetPoolStatistics();
                _logger.LogInformation($"Total entries into pool: {cardPool.TotalCardCount}");

                poolStats.PrintStatistics();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Exception while drawing card into new pool for statistics check.", new[] { new { CardSet = cardSet, RarityCode = rarityCode } });
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

                foreach(var set in _cardSets.Where(x => x.CycleCode == "full"))
                {
                    initTaskList.Add(set.Initialize());
                }

                await Task.WhenAll(initTaskList);

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

        public List<Card> GetAllExistingCards()
        {
            return _allExistingCards;
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

            var files = Directory.GetFiles(jsonDirectoryPath);

            foreach (var file in files)
            {
                if (file.Contains(".json"))
                {
                    using StreamReader reader = new StreamReader(file);
                    var json = reader.ReadToEnd();
                    var obj = JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);

                    if (obj != null)
                    {
                        allObjects.Add(obj);
                    }
                }
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
                    case "Miscellaneous":
                        return jsonObject.ToObject<Miscellaneous>(serializer);
                    case "SealedDeck":
                        return jsonObject.ToObject<SealedDeck>(serializer);
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
