using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class DeckController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IDeckRepository _deckRepository;
        private readonly IDeckManager _deckManager;

        public DeckController(IUserManager userManager, IDeckRepository deckRepository, IDeckManager deckManager)
        {
            _userManager = userManager;
            _deckRepository = deckRepository;
            _deckManager = deckManager;
        }

        [HttpPost]
        public async Task<ActionResult<Deck>> CreateDeck(string? deckName)
        {
            var userName = HttpContext?.User?.Identity?.Name;
            var user = await _userManager.GetUser(userName);

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return Problem("User not found.");
            }

            var deck = new Deck
            {
                DeckName = deckName,
                UserId = user.UserId,
                IsPublic = false
            };

            return await _deckRepository.CreateDeck(deck);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteDeck(int deckId)
        {
            var userName = HttpContext?.User?.Identity?.Name;
            var user = await _userManager.GetUser(userName);

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return Problem("User not found.");
            }

            var result = await _deckManager.DeleteDeck(deckId, user.UserId);

            if (!result)
            {
                return Problem("The deck could not be deleted.");
            }

            return Ok();
        }

        // renameDeck

        // toggleDeckVisibility

        [HttpPost]
        public async Task<ActionResult<List<DeckContent>>> AddCardsToDeck(AddCardsToDeckRequest request)
        {
            
            var userName = HttpContext?.User?.Identity?.Name;
            var user = await _userManager.GetUser(userName);

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return Problem("User not found.");
            }

            if (request.CardsToAdd.Count < 1 || request.CardsToAdd.Any(x => x.Count < 0) || request.CardsToAdd.Sum(x => x.Count) < 1)
            {
                return Problem("requested item list is empty or contains negative counts.");
            }

            var addedCards = await _deckManager.AddCardsToDeck(request.DeckId, user.UserId, request.CardsToAdd);

            return Ok(addedCards);

        }
        
        // addCardToDeck
            // call addCards



        // removeCards

        // removeCard
            // call removeCards

        // GetInventoryCardsMinusDeck
            // maaaybe?

        // GetDeck
            // specific deck w/ contents (deck must be either public or owned by logged-in user)

        // GetDeckVerbose
            // maaaybe?
            // specific deck w/ contents and card details

        // GetDeckList
            // list of all this user's decks

        // GetAllPublicDecks
            // list of all public decks
    }
}
