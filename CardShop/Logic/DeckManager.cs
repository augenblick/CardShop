using CardShop.Extensions;
using CardShop.Interfaces;
using CardShop.Models;
using System.ComponentModel;

namespace CardShop.Logic
{
    public class DeckManager : IDeckManager
    {
        private readonly IUserManager _userManager;
        private readonly IDeckRepository _deckRepository;
        private readonly ILogger _logger;
        private readonly IInventoryManager _inventoryManager;

        public DeckManager(IUserManager userManager, IDeckRepository deckRepository, ILogger<DeckManager> logger, IInventoryManager inventoryManager)
        {
            _userManager = userManager;
            _deckRepository = deckRepository;
            _logger = logger;
            _inventoryManager = inventoryManager;
        }

        public async Task<Deck> GetDeck(int deckId)
        {
            var deckTask =  _deckRepository.GetDeck(deckId);
            var deckContentsTask = _deckRepository.GetDeckContents(deckId);

            Task.WaitAll(deckTask, deckContentsTask);

            var deck = deckTask.Result;
            var deckContents = deckContentsTask.Result;

            if (deck == null || deck.DeckId < 1)
            {
                return new Deck();
            }

            deck.CardList = deckContents ?? new List<DeckContent>();

            return deck;
        }

        public async Task<Deck> GetDeckNoContents(int deckId)
        {
            var deck = await _deckRepository.GetDeck(deckId);

            return deck;
        }

        public async Task<bool> DeleteDeck(int deckId, int userId)
        {
            var deck = await GetDeckNoContents(deckId);

            if (deck.DeckId < 1 || deck.UserId != userId)
            {
                return false;
            }

            return await _deckRepository.DeleteDeck(deckId);
        }

        public async Task<(List<DeckContent>, string)> AddCardsToDeck(int deckId, int userId, List<DeckContent> cardsToAdd)
        {
            var errorMessage = string.Empty;
            var returnList = new List<DeckContent>();

            var deck = await GetDeck(deckId);
            if (deck == null || deck.DeckId < 1)
            {
                errorMessage = $"Deck with DeckId {deckId} not found.";
                return (returnList, errorMessage);
            }
            if (deck.UserId != userId)
            {
                errorMessage = $"The Deck with DeckId {deckId} is not owned by this user.";
                return (returnList, errorMessage);
            }

            var playerCards = await _inventoryManager.GetUserInventoryItems(userId, ProductType.Card);

            cardsToAdd = cardsToAdd.Consolidate();

            var cardsToAddToDeck = new List<DeckContent>();

            foreach(var card in cardsToAdd)
            {
                var ownedCard = playerCards.FirstOrDefault(x => x.Product.Code == card.CardProductCode);
                var inDeckCard = deck.CardList.FirstOrDefault(x => x.CardProductCode == card.CardProductCode);

                if (ownedCard == null)
                {
                    errorMessage = $"Card '{card.CardProductCode}' not found in user inventory.";
                    return (new List<DeckContent>(), errorMessage);
                }
                if (ownedCard!.Count - (inDeckCard?.Count ?? 0) < card.Count)
                {
                    errorMessage = $"Not enough of card '{card.CardProductCode}' in inventory.";
                    return (new List<DeckContent>(), errorMessage);
                }

                cardsToAddToDeck.Add(card);
            }

            var updated = await _deckRepository.UpsertDeckContents(deckId, cardsToAddToDeck);

            if (!updated) 
            {
                errorMessage = $"An error occurred and the requested card(s) could not be added to the deck.";
                _logger.LogError(errorMessage);
                return (new List<DeckContent>(), errorMessage); 
            }

            // TODO: return updated deck?
            return (cardsToAdd, errorMessage);
        }

        public async Task<(List<DeckContent>, string)> RemoveCardsFromDeck(int deckId, int userId, List<DeckContent> cardsToRemove)
        {
            var errorMessage = string.Empty;
            var returnList = new List<DeckContent>();
            var cardsToRemoveFromDeck = new List<DeckContent>();

            var deck = await GetDeck(deckId);
            if (deck == null || deck.DeckId < 1)
            {
                errorMessage = $"Deck with DeckId {deckId} not found.";
                return (returnList, errorMessage);
            }

            cardsToRemove = cardsToRemove.Consolidate();

            foreach (var thisCard in cardsToRemove)
            {
                var countToRemove = thisCard.Count;
                var cardProductCode = thisCard.CardProductCode;

                var inDeckCard = deck.CardList.FirstOrDefault(x => x.CardProductCode == cardProductCode);

                if (inDeckCard == null)
                {
                    errorMessage = $"Card '{cardProductCode}' not found in deck {deckId}.";
                    return (returnList, errorMessage);
                }
                
                if (inDeckCard.Count < countToRemove)
                {
                    errorMessage = $"Cannot remove {countToRemove}x of Card '{cardProductCode}'. Only {inDeckCard.Count}x exist in deck with DeckId {deckId}.";
                    return (returnList, errorMessage);
                }

                cardsToRemoveFromDeck.Add(new DeckContent
                {
                    CardProductCode = cardProductCode,
                    Count = inDeckCard.Count - countToRemove
                });
            }

            var updated = await _deckRepository.UpsertDeckContents(deckId, cardsToRemoveFromDeck);

            if (!updated)
            {
                errorMessage = $"An error occurred and the requested card(s) could not be added to the deck.";
                _logger.LogError(errorMessage);
                return (new List<DeckContent>(), errorMessage);
            }

            // remove any deck contents reduced to zero count
            var zeroCountCardsCleared = await _deckRepository.ClearZeroedDeckContents(deckId);

            return (cardsToRemoveFromDeck, errorMessage);
        }

        public async Task<List<Deck>> GetDecks(int? userId = null, bool? isPublic = null)
        {
            return await _deckRepository.GetDecks(userId, isPublic);
        }

        public async Task<(Deck, string)> ToggleDeckVisibility(int userId, int deckId)
        {
            var (deck, errorMessage) = await GetDeckValidateDeckOwner(userId, deckId);

            if (errorMessage != null) { return (deck, errorMessage); }

            deck.IsPublic = !deck.IsPublic;

            var updatedDeck = await _deckRepository.UpdateDeck(deck);

            if (updatedDeck == null)
            {
                errorMessage = $"The deck with deckId {deckId} could not be updated";
                _logger.LogError(errorMessage);
                return (deck, errorMessage);
            }

            return (updatedDeck, errorMessage);
        }

        public async Task<(Deck, string)> RenameDeck(int userId, int deckId, string newDeckName)
        {
            var (deck, errorMessage) = await GetDeckValidateDeckOwner(userId, deckId);

            if (errorMessage != null) { return (deck, errorMessage); }

            deck.DeckName = newDeckName;

            var updatedDeck = await _deckRepository.UpdateDeck(deck);

            if (updatedDeck == null)
            {
                errorMessage = $"The deck with deckId {deckId} could not be updated";
                _logger.LogError(errorMessage);
                return (deck, errorMessage);
            }

            return (updatedDeck, errorMessage);
        }

        private async Task<(Deck, string?)> GetDeckValidateDeckOwner(int userId, int deckId)
        {
            var errorMessage = string.Empty;
            var returnDeck = new Deck();
            var deck = await GetDeckNoContents(deckId);

            if (deck == null || deck.DeckId < 1)
            {
                errorMessage = $"Deck with deckId {deckId} not found.";
                return (returnDeck, errorMessage);
            }
            if (deck.UserId != userId)
            {
                errorMessage = $"The Deck with DeckId {deckId} is not owned by this user.";
                return (returnDeck, errorMessage);
            }

            return (deck, null);
        }
    }
}
