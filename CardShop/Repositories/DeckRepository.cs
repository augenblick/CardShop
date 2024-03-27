using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Models.Request;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CardShop.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public DeckRepository(IConfiguration configuration, ILogger<InventoryRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /*public async Task<List<Inventory>> GetUserInventory(int userId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            SqlMapper.AddTypeHandler(new CardSetCodeTypeHandler());

            var inventory = await dbConnection.QueryAsync<Inventory>($@"
                        SELECT *
                        FROM Inventory
                        WHERE UserId = @UserId
                        AND Count > 0",
                        new
                        {
                            UserId = userId
                        });

            return inventory.AsList();
        }*/

        public async Task<Deck> CreateDeck(Deck deck)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var insertedDeck = await dbConnection.QueryAsync<Deck>($@"
                        INSERT INTO Deck(DeckName, UserId, DeckType, IsPublic) 
                        VALUES(@DeckName, @UserId, @DeckType, @IsPublic)
                        RETURNING *;",
                        new
                        {
                            DeckName = deck.DeckName,
                            UserId = deck.UserId,
                            DeckType = deck.DeckType,
                            IsPublic = deck.IsPublic
                        });

            return insertedDeck?.FirstOrDefault();
        }

        public async Task<Deck> GetDeck(int deckId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var deck = await dbConnection.QueryFirstOrDefaultAsync<Deck>(@"
                        SELECT *
                        FROM Deck
                        WHERE DeckId = @DeckId
                        ", new { DeckId = deckId});

            return deck;
        }

        public async Task<List<DeckContent>> GetDeckContents(int deckId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            var deckContents = await dbConnection.QueryAsync<DeckContent>(@"
                        SELECT CardProductCode, Count
                        FROM DeckContent
                        WHERE DeckId = @DeckId
                        ", new { DeckId = deckId });

            return deckContents.AsList();
        }

        public async Task<bool> DeleteDeck(int deckId)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();

            try
            {
                var contentDeletedCount = await dbConnection.ExecuteAsync(@"
                        DELETE FROM DeckContent
                        WHERE DeckId = @DeckId", new { DeckId = deckId });

                var deckDeletedCount = await dbConnection.ExecuteAsync(@"
                        DELETE FROM Deck
                        WHERE DeckId = @DeckId", new { DeckId = deckId });

                if (deckDeletedCount < 1)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;

        }

        public async Task<bool> AddCardsToDeck(int deckId, List<DeckContent> cardsToAdd)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("CardShopConnectionString"));

            dbConnection.Open();
            using var transaction = dbConnection.BeginTransaction();

            try
            {
                foreach (var card in cardsToAdd)
                {
                    var contentDeletedCount = await dbConnection.ExecuteAsync(@"
                    INSERT OR REPLACE INTO DeckContent (DeckId, CardProductCode, Count)
                    VALUES (@DeckId, @CardProductCode, @Count)", new { DeckId = deckId, CardProductCode = card.CardProductCode, Count = card.Count });
                }
            }
            catch(Exception ex)
            {
                // TODO: log error
                transaction.Rollback();
            }

            transaction.Commit();

            // TODO: return card list?
            return true;
        }
    }
}
