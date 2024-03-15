using Azure;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Models.Response;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CardShop : ControllerBase
    {
        private readonly IShopManager _shopManager;
        private readonly ILogger _logger;


        public CardShop(IShopManager shopManager, ILogger<CardShop> logger)
        {
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

        [HttpPost]
        public async Task<PurchaseProductResponse> PurchaseProduct(PurchaseProductRequest request)
        {
            if (request.InventoryItems == null || request.InventoryItems.Count < 1)
            {
                return new PurchaseProductResponse {ErrorMessage = "The request list is empty!" };            
            }

            if (request.PurchaserId == 0)
            {
                return new PurchaseProductResponse { ErrorMessage = "A PurchaserId of 0 (the shopKeeper's id) is not allowed!" };
            }

            var items = await _shopManager.PurchaseInventory(request.PurchaserId, request.InventoryItems);

            if (items == null || items.Count < 1)
            {
                return new PurchaseProductResponse { ErrorMessage = "An error occurred while trying to make a pruchase from the shop!" };
            }

            return new PurchaseProductResponse { InventoryItems = items };
        }

        
    }
}
