namespace CardShop.Models
{
    public class Product
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public ProductType ProductType { get; set; }
        public List<Content> Contents { get; set; }

    }

    public enum ProductType
    {
        BoosterBox,
        BoosterPack
    }

    public class Content
    {
        public string Code { get; set; }
        public int Count { get; set; }
    }

}
