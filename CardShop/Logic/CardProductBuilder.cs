using CardShop.Constants;
using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardShop.Logic
{
    // TODO: a lot of room for optimization throughout

    public class CardProductBuilder : ICardProductBuilder
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private List<CardSet> _cardSets;

        private readonly Random _randomizer = new Random();

        public CardProductBuilder()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
                {
                    Converters = { new ProductConverter() }
                };
        }

        public List<Product> GetProducts(string productCode, CardSetCode cardSetCode = CardSetCode.undefined, int count = 1)
        {
            var returnList = new List<Product>();

            var cardSet = GetCardSetByCardSetCode(cardSetCode);

            if (cardSet == null)
            {
                cardSet = _cardSets.FirstOrDefault(x => x.Products.Any(y => y.Code == productCode));
            }

            if (cardSet == null) { return returnList; }

            var product = cardSet.Products.FirstOrDefault(x => x.Code == productCode);

            if (product != null)
            {
                for (int i = 0; i < count; i++)
                {
                    returnList.Add(product);
                }
            }

            return returnList;
        }

        public List<KeyValuePair<Product, int>> GetProductsByProductType(ProductType productType, CardSetCode cardSetCode, int count = 1)
        {
            var returnList = new List<KeyValuePair<Product, int>>();

            var cardSet = GetCardSetByCardSetCode(cardSetCode);

            if (cardSet == null) { return returnList; }

            var product = cardSet.Products.FirstOrDefault(x => x.ProductType == productType);

            if (product != null)
            {
                returnList.Add(new KeyValuePair<Product, int>(product, count));
            }

            return returnList;
        } 

        private CardSet GetCardSetByCardSetCode(CardSetCode cardSetCode)
        {
            return _cardSets.FirstOrDefault(x => x.SetCode == cardSetCode.GetCardSetCodeString());
        }

        // NNOTE: MOVE TO INVENTORY MANAGER
        // OPEN should accept a reference to an InventoryId, and should only be allowed if
        // the inventory is found in the requester's own inventory.
        // should update inventory and response should include the opened contents.
        //
        // PURCHASE should accept a reference to an InventoryId (or Ids), and should only be allowed
        // if the inventory item is found in the shop's inventory AND the requester has enough money.
        // Shop AND requester's inventory should be updated, and response should include purchased items
        public List<object> OpenPremiereProduct(Product product)
        {
            var premiereSet = _cardSets.FirstOrDefault(x => x.SetCode == "pr");

            return premiereSet.OpenProduct(product).Select(x => (object)x).AsList();
        }

        public bool TestCardSetRarityPool(CardSetCode cardSetCode, Enums.RarityCode rarityCode, int testCount, bool peekDontDraw)
        {
            try
            {
                var cardSetCodeName = cardSetCode.GetCardSetCodeString();
                var cardSet = GetCardSetByCardSetCode(cardSetCode);

                // check null

                var cardPool = new CardPool(Enums.RarityCode.F);

                for (int i = 0; i < testCount; i++)
                {
                    cardPool.AddCardNoDuplicates(cardSet.DrawRandomCardFromSet(rarityCode, peekDontDraw), 1);
                }

                var poolStats = cardPool.GetPoolStatistics();
                Console.WriteLine($"Total entries into pool: {cardPool.TotalCardCount}");

                poolStats.PrintStatistics();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
