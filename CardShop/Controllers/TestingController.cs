using CardShop.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TestingController : ControllerBase
    {

        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly IShopManager _shopManager;
        private readonly ILogger _logger;

        public TestingController(ICardProductBuilder cardProductBuilder, IShopManager shopManager, ILogger<CardShop> logger)
        {
            _cardProductBuilder = cardProductBuilder;
            _shopManager = shopManager;
            _logger = logger;
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

        [HttpPost]
        public bool ClearShopInventory()
        {
            return _shopManager.ClearShopInventory();
        }
    }
}
