using Azure;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Models;
using CardShop.Models.Response;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CardShop : ControllerBase
    {
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IShopManager _shopManager;
        private readonly ILogger _logger;

        public CardShop(ICardProductBuilder cardProductBuilder, IShopManager shopManager, ILogger<CardShop> logger)
        {
            _cardProductBuilder = cardProductBuilder;
            _shopManager = shopManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<GetShopInventoryResponse> GetShopInventory()
        {
            var response = new GetShopInventoryResponse();
            try
            {
                var inventory = await _shopManager.GetShopInventory();

                response.Inventory = inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while trying to return shop inventory.");
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}
