using System.Text.Json.Serialization;

namespace CardShop.Models
{
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(BoosterPack), typeDiscriminator: "BoosterPack")]
    [JsonDerivedType(typeof(BoosterBox), typeDiscriminator: "BoosterBox")]
    [JsonDerivedType(typeof(StarterDeck), typeDiscriminator: "StarterDeck")]
    [JsonDerivedType(typeof(EnhancedPack), typeDiscriminator: "EnhancedPack")]
    [JsonDerivedType(typeof(PromotionalSet), typeDiscriminator: "PromotionalSet")]
    [JsonDerivedType(typeof(StarterSet), typeDiscriminator: "StarterSet")]
    [JsonDerivedType(typeof(SealedDeck), typeDiscriminator: "SealedDeck")]
    [JsonDerivedType(typeof(AnthologySet), typeDiscriminator: "AnthologySet")]
    [JsonDerivedType(typeof(Card), typeDiscriminator: "Card")]
    public class Product : IEquatable<Product>
    {
        public string Code { get; set; }
        public Enums.CardSetCode SetCode { get; set; }
        public string Name { get; set; }
        public virtual decimal CostPer { get; set; }
        public ProductType ProductType { get; set; }
        public bool IsPurchasable { get; set; } = true;
        public List<Content> Contents { get; set; }

        public bool Equals(Product? other)
        {
            if (other == null) { return this == null; }

            return other.Code.ToLower() == this.Code.ToLower();
        }
    }

    public enum ProductType
    {
        BoosterBox,
        BoosterPack,
        StarterDeck,
        SealedDeck,
        Card,
        StarterSet,
        PromotionalSet,
        EnhancedPack,
        AnthologySet
    }

    public class BoosterBox : Product
    {
        public override decimal CostPer { get; set; } = 50.0M;
        //public List<Content> Contents { get; set; }
    }

    public class StarterDeck : Product
    {
        public override decimal CostPer { get; set; } = 9.95M;
        //public List<Content> Contents { get; set; }
    }

    public class StarterSet : Product
    {
        public override decimal CostPer { get; set; } = 19.95M;
    }

    public class BoosterPack : Product
    {
        public override decimal CostPer { get; set; } = 3.0M;
    }

    public class EnhancedPack : Product
    {
        public override decimal CostPer { get; set; } = 14.95M;
    }

    public class PromotionalSet : Product
    {
        public override decimal CostPer { get; set; } = 5.0M;
    }

    public class SealedDeck : Product
    {
        public override decimal CostPer { get; set; } = 19.95M;
    }
    public class AnthologySet : Product
    {
        public override decimal CostPer { get; set; } = 35.0M;
    }

    public class Content
    {
        public List<string>? SetCodes { get; set; }
        public string Code { get; set; }
        public int Count { get; set; }
        public RandomPickParameters RandomPickParameters { get; set; }
    }

    public class RandomPickParameters
    {
        public List<string>? OverallRarities { get; set; }
        public string? Side { get; set; }
    }
}
