namespace CardShop.Models
{
    public class BoosterPack : Product
    {
        public List<KeyValuePair<string, int>> ContentRarities { get; set; }
    }
}
