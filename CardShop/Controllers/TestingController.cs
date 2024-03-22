using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Models.Request;
using CardShop.Models.Response;
using CardShop.Repositories.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TestingController : ControllerBase
    {

        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IShopManager _shopManager;
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;
        private readonly IInventoryManager _inventoryManager;

        public TestingController(ICardProductBuilder cardProductBuilder, IShopManager shopManager, ILogger<CardShop> logger, IUserManager userManager, IInventoryManager inventoryManager)
        {
            _cardProductBuilder = cardProductBuilder;
            _shopManager = shopManager;
            _logger = logger;
            _userManager = userManager;
            _inventoryManager = inventoryManager;
        }

        [HttpPost]
        public async Task InitializeShop()
        {
            _shopManager.Initialize();
        }

        [HttpPost]
        public bool TestRarityPoolLogic(string rarityCode, int testCount, bool peekDontDraw = true, Enums.CardSetCode cardSetCode = Enums.CardSetCode.Premiere)
        {
            return _cardProductBuilder.TestCardSetRarityPool(cardSetCode, rarityCode.ToUpper(), testCount, peekDontDraw);
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
        public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            return await _inventoryManager.GetUserInventory(userId);
        }

        [HttpPost]
        public async Task<bool> SetUserBalance(int userId, decimal newBalance)
        {
            return await _userManager.SetUserBalance(userId, newBalance);
        }

        [HttpPost]
        public async Task<User> AddUser(string userName, decimal balance = 0.0M)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new User();
            }

            return await _userManager.AddUser(userName, balance);
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

                    response.ErrorMessage = message;
                    response.CountReturned = response.CountReturned + items.Sum(x => x.Count);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
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
    }
}
