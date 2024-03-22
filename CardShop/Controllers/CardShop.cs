using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using CardShop.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CardShop : ControllerBase
    {
        private readonly IShopManager _shopManager;
        private readonly ILogger _logger;
        private readonly IInventoryManager _inventoryManager;
        private readonly ICardProductBuilder _cardProductBuilder;


        public CardShop(IShopManager shopManager, ILogger<CardShop> logger, IInventoryManager inventoryManager, ICardProductBuilder cardProductBuilder)
        {
            _shopManager = shopManager;
            _logger = logger;
            _inventoryManager = inventoryManager;
            _cardProductBuilder = cardProductBuilder;
        }

        [HttpPost]
        public async Task<GetShopInventoryResponse> GetShopInventory(bool includeOutOfStock = false)
        {
            var response = new GetShopInventoryResponse();
            try
            {
                var inventory = await _shopManager.GetVerboseShopInventory(includeOutOfStock);

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
            if (request.InventoryItems.Any(x => x.Count < 0))
            {
                return new PurchaseProductResponse { ErrorMessage = "Negative counts are not allowed!!" };
            }

            if (request.InventoryItems == null || request.InventoryItems.Count < 1 || request.InventoryItems.Sum(x => x.Count) < 1)
            {
                return new PurchaseProductResponse {ErrorMessage = "The request list is empty!" };            
            }

            if (request.PurchaserId == 0)
            {
                return new PurchaseProductResponse { ErrorMessage = "A PurchaserId of 0 (the shopKeeper's id) is not allowed!" };
            }

            var (items, totalCost, remainingBalance, errorMessage) = await _shopManager.PurchaseInventory(request.PurchaserId, request.InventoryItems);

            if (items == null || items.Count < 1)
            {
                return new PurchaseProductResponse { ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ?  "An error occurred while trying to make a purchase from the shop!" : errorMessage };
            }

            return new PurchaseProductResponse { 
                RemainingUserBalance = remainingBalance,
                TotalCost = totalCost,
                InventoryItems = items 
            };
        }

        [HttpPost]
        public async Task<OpenInventoryProductsResponse> OpenInventoryProducts(OpenInventoryProductsRequest request)
        {
            if (request.UserId == 0)
            {
                return new OpenInventoryProductsResponse
                {
                    ErrorMessage = "Cannot open inventory for userId 0 (shop keeper)!"
                };
            }

            var watch = new Stopwatch();
            watch.Start();
            var (items, errorMessage) = await _inventoryManager.OpenInventoryProducts(request.UserId, request.InventoryProductsToOpen);

            watch.Stop();

            _logger.LogInformation($"total time to open products = {watch.ElapsedMilliseconds}ms");

            return new OpenInventoryProductsResponse
            {
                ErrorMessage = errorMessage,
                OpenedItems = items,
                TotalItems = items.Sum(x => x.Count)
            };
        }

        [HttpPost]
        public GetAllAvailableProductInfoResponse GetAllAvailableProductInfo(bool includeCards = false)
        {
            var products = _cardProductBuilder.GetAllExistingProducts();
            var cards = includeCards ? _cardProductBuilder.GetAllExistingCards() : new List<Card>();

            return new GetAllAvailableProductInfoResponse
            {
                Products = products,
                Cards = cards
            };
        }

        [HttpGet]
        public ActionResult<GetAllAvailableCardSetInfoResponse> GetAllAvailableCardSetInfo()
        {
            var response = new GetAllAvailableCardSetInfoResponse();
            try
            {
                var cardSetsInfo = _cardProductBuilder.GetAllAvailableCardSetInfo();

                response.CardSetsInfo = cardSetsInfo;
                response.InfoCount = cardSetsInfo.Count;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all set data.");
                return Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }

            return response.InfoCount < 1 ? NotFound(response) : Ok(response);
        }

        [HttpPost]
        public async Task<Product> GetProductInfo(string productCode)
        {
            return _cardProductBuilder.GetProduct(productCode);
        }

        [HttpPost]
        public async Task<List<Product>> GetProductInfoMany(List<string> productCodes)
        {
            var returnList = new List<Product>();

            foreach(var productCode in productCodes)
            {
                var product = _cardProductBuilder.GetProduct(productCode);

                if (product == null || string.IsNullOrWhiteSpace(product.Code))
                {
                    _logger.LogError($"Product '{productCode}' not found!");
                    continue;
                }

                returnList.Add(product);
            }

            return returnList;
        }

    }
}
