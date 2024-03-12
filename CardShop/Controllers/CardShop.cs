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

        public CardShop(ICardProductBuilder cardProductBuilder, IShopManager shopManager)
        {
            _cardProductBuilder = cardProductBuilder;
            _shopManager = shopManager;
        }

        [HttpPost]
        public async Task<GetShopInventoryResponse> GetShopInventory()
        {
            var response = await _shopManager.GetShopInventory();

            return new GetShopInventoryResponse
            {
                Inventory = response,
            };
        }

        [HttpPost]
        public async Task InitializeShop()
        {
            _shopManager.Initialize();
        }

        [HttpPost]
        public bool TestRarityPoolLogic(Enums.RarityCode rarityCode, int testCount, bool peekDontDraw = true, Enums.CardSetCode cardSetCode = Enums.CardSetCode.Premiere)
        {
            return _cardProductBuilder.TestCardSetRarityPool(Enums.CardSetCode.Premiere, rarityCode, testCount, peekDontDraw);
        }

    }
}
