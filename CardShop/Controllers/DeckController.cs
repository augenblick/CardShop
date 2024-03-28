using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using Dapper;
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
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
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
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
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

        

        [HttpPost]
        public async Task<ActionResult<List<DeckContent>>> AddCardsToDeck(AddCardsToDeckRequest request)
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            if (request.CardsToAdd.Count < 1 || request.CardsToAdd.Any(x => x.Count < 0) || request.CardsToAdd.Sum(x => x.Count) < 1)
            {
                return Problem("requested item list is empty or contains negative counts.");
            }

            if (request.CardsToAdd.Any(x => string.IsNullOrWhiteSpace(x.CardProductCode)))
            {
                return Problem("requested item list contains empty CardProductCode(s).");
            }

            var (addedCards, errorMessage) = await _deckManager.AddCardsToDeck(request.DeckId, user.UserId, request.CardsToAdd);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Problem(errorMessage);
            }

            return Ok(addedCards);
        }

        [HttpPost]
        public async Task<ActionResult<List<DeckContent>>> AddSingleCardToDeck(AddSingleCardToDeckRequest request)
        {
            return await AddCardsToDeck(new AddCardsToDeckRequest
            {
                DeckId = request.DeckId,
                CardsToAdd = new List<DeckContent> { new DeckContent
                    {
                        CardProductCode = request.CardProductCode,
                        Count = 1
                    }
                }
            });
        }

        [HttpPost]
        public async Task<ActionResult<List<DeckContent>>> RemoveCardsFromDeck(AddCardsToDeckRequest request)
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            if (request.CardsToAdd.Count < 1 || request.CardsToAdd.Any(x => x.Count < 0) || request.CardsToAdd.Sum(x => x.Count) < 1)
            {
                return Problem("requested item list is empty or contains negative counts.");
            }

            if (request.CardsToAdd.Any(x => string.IsNullOrWhiteSpace(x.CardProductCode)))
            {
                return Problem("requested item list contains empty CardProductCode(s).");
            }

            var (removedCards, errorMessage) = await _deckManager.RemoveCardsFromDeck(request.DeckId, user.UserId, request.CardsToAdd);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Problem(errorMessage);
            }

            return Ok(removedCards);
        }

        [HttpPost]
        public async Task<ActionResult<List<DeckContent>>> RemoveSingleCardFromDeck(AddSingleCardToDeckRequest request)
        {
            return await RemoveCardsFromDeck(new AddCardsToDeckRequest
            {
                DeckId = request.DeckId,
                CardsToAdd = new List<DeckContent> { new DeckContent
                    {
                        CardProductCode = request.CardProductCode,
                        Count = 1
                    }
                }
            });
        }

        [HttpGet]
        public async Task<ActionResult<Deck>> GetDeck(int deckId)
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            var deck = await _deckManager.GetDeck(deckId);

            if (deck == null || deck.DeckId < 1)
            {
                return NotFound(deck);
            }

            if (!deck.IsPublic && deck.UserId != user.UserId)
            {
                return Problem($"This deck is private and is not owned by UserId {user.UserId}.");
            }

            return Ok(deck);
        }

        [HttpGet]
        public async Task<ActionResult<List<Deck>>> GetUserDecks()
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            var allDecks = await _deckManager.GetDecks(user.UserId);

            return allDecks;
        }

        [HttpGet]
        public async Task<ActionResult<List<Deck>>> GetPublicDecks()
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            var allDecks = await _deckManager.GetDecks(null);

            return allDecks.Where(x => x.IsPublic || x.UserId == user.UserId).AsList();
        }

        [HttpPost]
        public async Task<ActionResult<Deck>> ToggleDeckVisibility(int deckId)
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            var (updatedDeck, errorMessage) = await _deckManager.ToggleDeckVisibility(user.UserId, deckId);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Problem(errorMessage);
            }

            return Ok(updatedDeck);
        }

        [HttpPost]
        public async Task<ActionResult<Deck>> RenameDeck(int deckId, string newDeckName)
        {
            var user = await _userManager.GetUser(HttpContext);

            if (user == null)
            {
                return Problem("User not found.");
            }

            var (updatedDeck, errorMessage) = await _deckManager.RenameDeck(user.UserId, deckId, newDeckName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Problem(errorMessage);
            }

            return Ok(updatedDeck);
        }
    }
}
