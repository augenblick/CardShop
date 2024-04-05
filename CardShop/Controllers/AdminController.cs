using CardShop.ConfigurationClasses;
using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Models.Response;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {

        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IShopManager _shopManager;
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IDeckManager _deckManager;
        private readonly IDeckRepository _deckRepository;

        public AdminController(ICardProductBuilder cardProductBuilder, IShopManager shopManager, ILogger<CardShop> logger, 
            IUserManager userManager, IInventoryManager inventoryManager, IDeckManager deckManager, IDeckRepository deckRepository)
        {
            _cardProductBuilder = cardProductBuilder;
            _shopManager = shopManager;
            _logger = logger;
            _userManager = userManager;
            _inventoryManager = inventoryManager;
            _deckManager = deckManager;
            _deckRepository = deckRepository;
        }

        [HttpPost]
        public async Task InitializeShop()
        {
            _shopManager.Initialize();
        }

        [HttpPost]
        public bool TestRarityPoolLogic(List<string> rarityCodes, int testCount, bool peekDontDraw = true, Enums.CardSetCode cardSetCode = Enums.CardSetCode.Premiere)
        {
            return _cardProductBuilder.TestCardSetRarityPool(cardSetCode, rarityCodes.Select(x => x.ToUpper()).AsList(), testCount, peekDontDraw);
        }

        [HttpPost]
        public bool ClearShopInventory()
        {
            return _shopManager.ClearShopInventory();
        }

        [HttpPost]
        public async Task<User> GetUser(int userId)
        {
            return await _userManager.GetUser(userId);
        }

        [HttpPost]
        public async Task<List<Inventory>> GetUserInventoryByUserId(int userId)
        {
            return await _inventoryManager.GetUserInventory(userId);
        }

        [HttpPost]
        public async Task<bool> SetUserBalance(int userId, decimal newBalance)
        {
            return await _userManager.SetUserBalance(userId, newBalance);
        }

        [HttpDelete]
        public async Task<bool> DeleteDeck(int deckId)
        {
            var deck = await _deckManager.GetDeckNoContents(deckId);

            if (deck.DeckId < 1)
            {
                return false;
            }

            return await _deckRepository.DeleteDeck(deckId);
        }

        [HttpPost]
        public async Task<User> SetUserRoleById(int userId, Role role)
        {
            return await _userManager.SetUserRole(userId, role);
        }

        [HttpPost]
        public async Task<User> SetUserRoleByUsername(string username, Role role)
        {
            return await _userManager.SetUserRole(username, role);
        }

        [HttpPost]
        public async Task<bool> DeleteUser(int userId)
        {
            return await _userManager.DeleteUser(userId);
        }

        /// <summary>
        /// Opens multiple products at once, but doesn't return products in response 
        /// to keep from bombarding Swagger upon opening very large quantities.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="repeatOrderTimes">repeat the order this many times</param>
        /// <param name="openOrder">List of products and counts to open</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<OpenMultipleProductTestResponse> OpenMultipleProductTest(int userId, int repeatOrderTimes, List<ProductReference> openOrder )
        {
            var response = new OpenMultipleProductTestResponse();

            try
            {
                var timer = new Stopwatch();
                timer.Start();

                for (int i = 0; i < repeatOrderTimes; i++)
                {
                    var (items, message) = await _inventoryManager.OpenInventoryProducts(userId, openOrder);

                    response.CountReturned = response.CountReturned + items.Sum(x => x.Count);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        response.ErrorMessage = message;
                        break;
                    }
                }

                timer.Stop();
                _logger.LogInformation($"OpenMultipleProductTest operation took {timer.ElapsedMilliseconds}ms.");
            }
            catch(Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        [HttpPost]
        public async Task<bool> DeleteUserInventory(int userId)
        {
            return _inventoryManager.ClearUserInventory(userId);
        }

        [HttpGet]
        public async Task<ActionResult> GetUserInventoryStats(int userId, string setCode = null)
        {
            try
            {
                var cardLists = new List<List<InventoryItem>>();

                var userInventory = await _inventoryManager.GetUserInventory(userId);
                var userInventoryItems = _inventoryManager.InventoryItemsFromInventory(userInventory.Where(x => Enums.CardSetHelpers.GetCardSetCodeString(x.SetCode) == setCode || setCode == null).AsList());

                foreach (var item in userInventoryItems)
                {
                    if (item.Product is Card)
                    {
                        var card = item.Product as Card;
                        var existingList = cardLists.FirstOrDefault(x => x.FirstOrDefault(c => ((Card)c.Product).RarityCode == card?.RarityCode) != null);

                        if (existingList == null)
                        {
                            existingList = [item];

                            cardLists.Add(existingList);
                            continue;
                        }
                        existingList.Add(item);
                    }
                }

                foreach (var cardList in cardLists)
                {
                    var listRarity = ((Card)cardList.First().Product).RarityCode;
                    var listCount = cardList.Sum(x => x.Count);

                    _logger.LogInformation($"{listCount}x cards of rarity '{listRarity}' (an average of {listCount / cardList.Count}x per card)");

                }

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                return Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public List<string> GetCardsByName(List<string> cardNames, string rarityCode, bool makeFoil, string productCodeSuffix, string setCode)
        {
            var returnList = new List<string>();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new SnakeCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            if (cardNames != null)
            {
                foreach (var cardName in cardNames)
                {
                    // search for matching card
                    var foundCard = _cardProductBuilder.GetCardByName(cardName);

                    if (foundCard != null)
                    {
                        foundCard.RarityCode = string.IsNullOrWhiteSpace(rarityCode) ? foundCard.RarityCode : rarityCode;
                        foundCard.Code = string.IsNullOrWhiteSpace(productCodeSuffix) ? foundCard.Code : foundCard.Code + productCodeSuffix;
                        foundCard.SetCode = string.IsNullOrWhiteSpace(setCode) ? foundCard.SetCode : setCode;
                        foundCard.IsFoil = makeFoil;


                        returnList.Add(JsonConvert.SerializeObject(foundCard, settings));
                    }
                    else
                    {
                        _logger.LogInformation($"No card found for card name '{cardName}'");
                    }
                }
            }

            return returnList;
        }
    }
}
