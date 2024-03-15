﻿using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Repositories.Models;
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
        public bool TestRarityPoolLogic(Enums.RarityCode rarityCode, int testCount, bool peekDontDraw = true, Enums.CardSetCode cardSetCode = Enums.CardSetCode.Premiere)
        {
            return _cardProductBuilder.TestCardSetRarityPool(Enums.CardSetCode.Premiere, rarityCode, testCount, peekDontDraw);
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
    }
}
