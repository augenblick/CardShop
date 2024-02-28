namespace CardShop.Repositories.Models
{
    public class Card
    {
        public int CardId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Text { get; set; }
        public string Rarity { get; set; }
        public string Set {  get; set; }
        public string Side {  get; set; }
        
    }
}
