using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Models;
using CardShop.Repositories.Models;
using Microsoft.AspNetCore.Mvc;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CardShop : ControllerBase
    {
        private readonly ICardTestRepository _cardTestRepository;
        private readonly ICardProductBuilder _cardProductBuilder;

        public CardShop(ICardTestRepository cardTestRepository, ICardProductBuilder cardProductBuilder)
        {
            _cardTestRepository = cardTestRepository;
            _cardProductBuilder = cardProductBuilder;
        }

        [HttpGet]
        public async Task<Card> GetCardByCardId(int Id)
        {
            var card = await _cardTestRepository.GetCardByCardIdAsync(Id);
            return card;
        }

        [HttpGet]
        public async Task<IEnumerable<Card>> GetShopInventory()
        {
            var cards = await _cardTestRepository.GetAllCardsAsync();
            return cards;
        }

        /// <summary>
        /// Currently supports only Premiere, New Hope, Hoth, Dagobah, Cloud City, and Special Edition
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cardSetName"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<CardPack> GetCardPacks(int count, string cardSetName)
        {
            return _cardProductBuilder.GetCardPacks(count, cardSetName);
        }
    }
}
