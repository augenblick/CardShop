﻿using System.Text.Json.Serialization;

namespace CardShop.Models
{
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    //[JsonPolymorphic(TypeDiscriminatorPropertyName = "$productType")]
    [JsonDerivedType(typeof(BoosterPack), typeDiscriminator: "BoosterPack")]
    [JsonDerivedType(typeof(BoosterBox), typeDiscriminator: "BoosterBox")]
    [JsonDerivedType(typeof(Card), typeDiscriminator: "Card")]
    public class Product
    {
        public string Code { get; set; }
        public Enums.CardSetCode SetCode { get; set; }
        public string Name { get; set; }
        public decimal CostPer { get; set; }
        public ProductType ProductType { get; set; }
    }

    public enum ProductType
    {
        BoosterBox,
        BoosterPack,
        Card
    }

    public class BoosterBox : Product
    {
        public List<Content> Contents { get; set; }
    }

    public class Content
    {
        public string Code { get; set; }
        public int Count { get; set; }
    }

    public class BoosterPack : Product
    {
        public List<PackContentSpec> PackContentSpecs { get; set;}
    }

    public class PackContentSpec
    {
        public int Count { get; set; }
        public string OverallRarity { get; set; }
    }
}
