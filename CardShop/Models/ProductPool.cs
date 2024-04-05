namespace CardShop.Models
{
    public class Pool<T> where T : IEquatable<T>
    {
        private List<PoolNode> _items = new List<PoolNode>();
        private Random _random = new Random();
        public int TotalItemCount
        {
            get { return _items.Sum(item => item.Duplicates); }
            set { }
        }

        public string PoolRarityCode { get; set; }

        public Pool(string poolRarityCode)
        {
            PoolRarityCode = poolRarityCode;
        }

        public Pool(string rarityCode, List<KeyValuePair<T, int>> items)
        {
            PoolRarityCode = rarityCode;
            foreach(var item in items)
            {
                Add(item.Key, item.Value);
            }
        }

        public Pool(List<Pool<T>> pools, string poolRarityCode)
        {
            PoolRarityCode = poolRarityCode;

            foreach (var pool in pools)
            {
                AppendNodes(pool.GetAllNodes());
            }
        }

        public void Add(T item, int count)
        {
            _items.Add(new PoolNode(item, count));
        }

        /// <summary>
        /// Checks for prior existance of given item within the pool and simply adds to the count if found.
        /// Otherwise creates a new entry as usual.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        public void AddItemNoDuplicates(T item, int count)
        {
            var existingItemIndex = _items.FindIndex(x => x.Equals(item));

            if (existingItemIndex < 0) { Add(item, count); return; }

            _items[existingItemIndex].Duplicates += count;
        }

        public void AppendNodes(List<PoolNode> nodes)
        {
            if (nodes == null) { return; }
            _items.AddRange(nodes);
        }

        public List<PoolNode> GetAllNodes()
        {
            return _items;
        }

        public T? Draw()
        {
            return Draw(false);
        }

        public T? Peek()
        {
            return Draw(true);
        }

        public List<T> Peek(int count)
        {
            var list = new List<T>();

            if (count < 1) { return list; }

            for (int i = 0; i < count; i++)
            {
                list.Add(Peek());
            }

            return list;
        }

        private T? Draw(bool peekWithoutDrawing)
        {
            if (_items.Count < 1) 
            { 
                RefreshPool();
            }

            double totalWeight = _items.Sum(card => card.Duplicates);
            double randomNumber = _random.NextDouble() * totalWeight;

            double cumulativeWeight = 0.0;

            foreach (PoolNode item in _items)
            {
                cumulativeWeight += item.Duplicates;

                if (randomNumber < cumulativeWeight)
                {
                    if (item.Duplicates > 0)
                    {
                        if (!peekWithoutDrawing)
                        {
                            item.Duplicates--;
                        }

                        return item.PoolItem;
                    }
                }
            }

            StaticHelpers.Logger.LogError("Unable to draw an item as requested.");
            return default(T);
        }

        public void RefreshPool()
        {
            foreach (PoolNode item in _items)
            {
                // Reset duplicates count to its original value
                item.Duplicates = item.InitialDuplicates;
            }
        }

        public class PoolNode
        {
            public T PoolItem { get; set; }
            public int Duplicates { get; set; } // Number of duplicates of this item in the pool
            public int InitialDuplicates { get; set; }

            public PoolNode(T poolItem, int duplicates)
            {
                PoolItem = poolItem;
                Duplicates = duplicates;
                InitialDuplicates = duplicates;
            }
        }


        public class PoolStatistics
        {
            public List<StatisticsEntry> PerEntryStatistics { get; set; } = new List<StatisticsEntry>();
            public int TotalItemCount { get; set; }

            public void PrintStatistics()
            {
                StaticHelpers.Logger.LogInformation($"Total Items in pool: '{TotalItemCount}'");
                
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
