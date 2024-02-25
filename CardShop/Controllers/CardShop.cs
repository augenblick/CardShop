using CardShop.DbModels;
using CardShop.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CardShop : ControllerBase
    {
        private readonly ICardTestRepository _cardTestRepository;

        public CardShop(ICardTestRepository cardTestRepository) 
        { 
            _cardTestRepository = cardTestRepository;
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
    }
}
