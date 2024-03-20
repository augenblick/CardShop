using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CardShop.Logic
{
    // TODO: a lot of room for optimization throughout

    public class CardProductBuilder : ICardProductBuilder
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private List<CardSet> _cardSets;

        private readonly Random _randomizer = new Random();
        private readonly ILogger _logger;
        private List<Product> _allPossibleProducts = new List<Product>();

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

        public List<InventoryItem> OpenProduct(Product product)
        {
            if (product == null)
            {
                return new List<InventoryItem>();
            }

            var cardSet = GetCardSetByCardSetCode(product.SetCode);

            if (product is BoosterPack)
            {
                var contents = cardSet.OpenBoosterPack(product as BoosterPack);

                return contents;
            }

            // non-BoosterPack product
            var returnProductList = new List<InventoryItem>();
            foreach (var content in ((BoosterBox)product).Contents)
            {
                var setForThisContent = cardSet;
                if (!string.IsNullOrWhiteSpace(content.SetCode))
                {
                    // a specific set is defined for this content, so use it.
                    setForThisContent = GetCardSetByCardSetCode(CardSetHelpers.GetCardSetCode(content.SetCode));
                }

                var selectedContent = setForThisContent.GetCardSetProduct(content.Code);

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
                    StaticHelpers.Logger.LogError($"No matching product '{product.Code}' found in set '{setForThisContent.SetCode}' while opening products!");
                }
            }

            // consolidate any like Contents
            var consolidatedList = returnProductList
                .GroupBy(x => x.Product.Code)
                .Select(y => new InventoryItem
                {
                    Product = y.First().Product,
                    Count = y.Sum(c => c.Count)
                }).AsList();

            return consolidatedList;
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
                    var cost = ((BoosterBox)product).Contents.FirstOrDefault()?.Count * perPackCost * 0.75M;
                    product.CostPer = cost ?? 0.00M;
                }
                else if (product is BoosterPack)
                {
                    product.CostPer = perPackCost;
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

            var product = cardSet.Products.FirstOrDefault(x => x.ProductType == productType);

            if (product != null)
            {
                // TODO: cost and setcode will eventually be defined per product within each set.json
                var perPackCost = 2.50M;

                product.SetCode = product.SetCode == CardSetCode.undefined ? cardSetCode : product.SetCode;

                if (product is BoosterBox)
                {
                    var cost = ((BoosterBox)product).Contents.FirstOrDefault()?.Count * perPackCost * 0.75M;
                    product.CostPer = cost ?? 0.00M;
                }
                else if (product is BoosterPack)
                {
                    product.CostPer = perPackCost;
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
                    cardPool.AddCardNoDuplicates(cardSet.DrawRandomCardFromSet(rarityCode, peekDontDraw), 1);
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
            return _allPossibleProducts;
        }

        private void BuildAllExistingProducts()
        {
            var existingProducts = new List<Product>();

            foreach (var set in _cardSets.OrderBy(x => x.Position))
            {
                existingProducts.AddRange(set.Products.OrderBy(x => x.ProductType));
            }

            _allPossibleProducts = existingProducts.AsList();
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
