using Dapper;
using CardShop.Interfaces;
using Microsoft.Data.Sqlite;
using CardShop.Repositories.Models;
using CardShop.Constants;

namespace CardShop.Repositories
{
    public class CardTestRepository : ICardTestRepository
    {
        private readonly IConfiguration _configuration;

        public CardTestRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Card> GetCardByCardIdAsync(int id)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("DbConnectionString"));

            var card = await dbConnection.QuerySingleOrDefaultAsync<Card>($@"
                        SELECT * 
                        FROM CardsTest 
                        WHERE CardId=@CardId;", 
                        
                        new { CardId = id});

            return card;
        }

        public async Task<IEnumerable<Card>> GetAllCardsAsync()
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("DbConnectionString"));

            var cards = await dbConnection.QueryAsync<Card>($@"
                        SELECT * 
                        FROM CardsTest;");

            return cards;
        }

        public async Task<IEnumerable<Card>> GetCardSet(string setName)
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("DbConnectionString-CardDump"));

            var cards = await dbConnection.QueryAsync<Card>($@"
                        SELECT * 
                        FROM Cards
                        WHERE [Set] = @setName;", new { setName = setName});

            return cards;
        }

        public IEnumerable<Card> GetCardSets()
        {
            using var dbConnection = new SqliteConnection(_configuration.GetValue<string>("DbConnectionString-CardDump"));

            // TODO: un-hardcode this query
            var cards = dbConnection.Query<Card>($@"
                        SELECT * 
                        FROM Cards
                        WHERE [Set] IN ('{CardSetConstants.Premiere}', '{CardSetConstants.NewHope}', '{CardSetConstants.Hoth}', '{CardSetConstants.Dagobah}', '{CardSetConstants.CloudCity}', '{CardSetConstants.SpecialEdition}');");
            
            return cards;

        }
    }
}
