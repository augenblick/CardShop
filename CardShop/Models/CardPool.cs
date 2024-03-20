namespace CardShop.Models
{
    public class CardPool
    {
        private List<CardPoolNode> _cards = new List<CardPoolNode>();
        private Random _random = new Random();
        public int TotalCardCount
        {
            get { return _cards.Sum(card => card.Duplicates); }
            set { }
        }

        public string PoolRarityCode { get; set; }

        public CardPool(string poolRarityCode)
        {
            PoolRarityCode = poolRarityCode;
        }

        public CardPool(string rarityCode, List<KeyValuePair<Card, int>> cards)
        {
            PoolRarityCode = rarityCode;
            foreach(var card in cards)
            {
                AddCard(card.Key, card.Value);
            }
        }

        public void AddCard(Card card, int count)
        {
            _cards.Add(new CardPoolNode(card, count));
        }

        /// <summary>
        /// Checks for prior existance of given card within the pool and simply adds to the count if found.
        /// Otherwise creates a new entry as usual.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="count"></param>
        public void AddCardNoDuplicates(Card card, int count)
        {
            var existingCardIndex = _cards.FindIndex(x => x.Card.Name == card.Name && x.Card.SideCode == card.SideCode);

            if (existingCardIndex < 0) { AddCard(card, count); return; }

            var existingCardEntry = _cards[existingCardIndex];

            existingCardEntry.Duplicates += count;
        }

        public Card DrawCard()
        {
            var card = DrawCard(false);

            return card;
        }

        public Card PeekCard()
        {
            return DrawCard(true);
        }

        public List<Card> PeekCards(int count)
        {
            var list = new List<Card>();

            if (count < 1) { return list; }

            for (int i = 0; i < count; i++)
            {
                list.Add(PeekCard());
            }

            return list;
        }

        private Card DrawCard(bool peekWithoutDrawing)
        {
            double totalWeight = _cards.Sum(card => card.Duplicates);
            double randomNumber = _random.NextDouble() * totalWeight;

            if (_cards.Count < 1 ) { RefreshPool(); }

            foreach (CardPoolNode card in _cards)
            {
                if (randomNumber < card.Duplicates)
                {
                    if (card.Duplicates > 0)
                    {
                        if (!peekWithoutDrawing)
                        {
                            card.Duplicates--;
                        }

                        return card.Card;
                    }
                }
                randomNumber -= card.Duplicates;
            }

            StaticHelpers.Logger.LogError("Unable to draw a card as requested.");
            return null;
        }

        public void RefreshPool()
        {
            foreach (CardPoolNode card in _cards)
            {
                // Reset duplicates count to its original value
                card.Duplicates = card.InitialDuplicates;
            }
        }

        public PoolStatistics GetPoolStatistics()
        {
            var stats = new PoolStatistics();
            double totalWeight = _cards.Sum(card => card.Duplicates);

            stats.TotalCardCount = TotalCardCount;

            foreach (var entry in _cards)
            {
                stats.PerEntryStatistics.Add(new StatisticsEntry
                {
                    CardName = entry.Card.Name, 
                    CardRarity = entry.Card.RarityCode.ToString(),
                    PercentOfTotal = (double)entry.Duplicates / totalWeight
                }); 
            }

            return stats;
        }

        public class CardPoolNode
        {
            public Card Card { get; set; }
            public int Duplicates { get; set; } // Number of duplicates of this card in the pool
            public int InitialDuplicates { get; set; }

            public CardPoolNode(Card card, int duplicates)
            {
                Card = card;
                Duplicates = duplicates;
                InitialDuplicates = duplicates;
            }
        }


        public class PoolStatistics
        {
            public List<StatisticsEntry> PerEntryStatistics { get; set; } = new List<StatisticsEntry>();
            public int TotalCardCount { get; set; }

            public void PrintStatistics()
            {
                StaticHelpers.Logger.LogInformation($"Total Cards in pool: '{TotalCardCount}'");
                
                foreach(var breakdown in GetStatBreakdown())
                {
                    StaticHelpers.Logger.LogInformation(breakdown.ToString());
                }

                foreach (var entry in PerEntryStatistics)
                {
                    StaticHelpers.Logger.LogInformation(entry.ToString());
                }
            }

            private List<KeyValuePair<string, string>> GetStatBreakdown()
            {
                var returnList = new List<KeyValuePair<string, string>>();

                var rarities = new List<string>();
                foreach(var entry in PerEntryStatistics)
                {
                    if (!rarities.Any(x => x == entry.CardRarity))
                    {
                        rarities.Add(entry.CardRarity);
                    }
                }

                foreach (var rarity in rarities)
                {
                    var percentages = PerEntryStatistics.Where(x => x.CardRarity == rarity).Select(y => y.PercentOfTotal);

                    var maxPercent = percentages.Max();
                    var minPercent = percentages.Min();
                    var avgPercent = percentages.Average();

                    returnList.Add(new KeyValuePair<string, string>($"Max Percentage for rarity '{rarity}'", maxPercent.ToString()));
                    returnList.Add(new KeyValuePair<string, string>($"Min Percentage for rarity '{rarity}'", minPercent.ToString()));
                    returnList.Add(new KeyValuePair<string, string>($"Avg Percentage for rarity '{rarity}'", avgPercent.ToString()));
                }

                return returnList;
            }

        }

        public class StatisticsEntry
        {
            public string CardName { get; set; }
            public string CardRarity { get; set; }
            public double PercentOfTotal { get; set; }

            public override string ToString()
            {
                return $">> CardName: '{CardName}', CardRarity: '{CardRarity}', PercentOfTotal: '{PercentOfTotal}'";
            }
        }
    }
}
