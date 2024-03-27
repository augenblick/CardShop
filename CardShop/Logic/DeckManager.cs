using CardShop.Interfaces;
using CardShop.Models;

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

            if (deck.DeckId < 1)
            {
                // TODO: log error
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

        public async Task<List<DeckContent>> AddCardsToDeck(int deckId, int userId, List<DeckContent> cardsToAdd)
        {
            var deck = await GetDeck(deckId);
            if (deck.DeckId < 1)
            {
                // TODO: deck not found!
            }

            // TODO: make sure this is returning only cards
            var playerCards = await _inventoryManager.GetUserInventoryItems(userId, ProductType.Card);

            // TODO: consolidate cardsToAdd

            var cardsToAddToDeck = new List<DeckContent>();

            foreach(var card in cardsToAdd)
            {
                var ownedCard = playerCards.FirstOrDefault(x => x.Product.Code == card.CardProductCode);
                var inDeckCard = deck.CardList.FirstOrDefault(x => x.CardProductCode == card.CardProductCode);

                if (ownedCard == null)
                {
                    // TODO: card not found
                }
                if (ownedCard!.Count - (inDeckCard?.Count ?? 0) < card.Count)
                {
                    // TODO: not enough of requested card!
                }

                cardsToAddToDeck.Add(card);
            }

            var updated = await _deckRepository.AddCardsToDeck(deckId, cardsToAddToDeck);

            if (!updated) 
            { 
                // TODO: return error message?
                return new List<DeckContent>(); 
            }

            // TODO: return updated deck?
            return cardsToAdd;
        }
    }
}
