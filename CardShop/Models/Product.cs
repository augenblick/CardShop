﻿using System.Text.Json.Serialization;

namespace CardShop.Models
{
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(BoosterPack), typeDiscriminator: "BoosterPack")]
    [JsonDerivedType(typeof(BoosterBox), typeDiscriminator: "BoosterBox")]
    [JsonDerivedType(typeof(StarterDeck), typeDiscriminator: "StarterDeck")]
    [JsonDerivedType(typeof(Card), typeDiscriminator: "Card")]
    public class Product
    {
        public string Code { get; set; }
        public Enums.CardSetCode SetCode { get; set; }
        public string Name { get; set; }
        public decimal CostPer { get; set; }
        public ProductType ProductType { get; set; }
        public bool IsPurchasable { get; set; } = true;
        public List<Content> Contents { get; set; }
    }

    public enum ProductType
    {
        BoosterBox,
        BoosterPack,
        StarterDeck,
        SealedDeck,
        Card,
        Miscellaneous
    }

    public class BoosterBox : Product
    {
        //public List<Content> Contents { get; set; }
    }

    public class StarterDeck : Product
    {
        //public List<Content> Contents { get; set; }
    }

    public class Content
    {
        public string? SetCode { get; set; }
        public string Code { get; set; }
        public int Count { get; set; }
        public RandomPickParameters RandomPickParameters { get; set; }
    }

    public class BoosterPack : Product
    {
        //public List<Content> PackContentSpecs { get; set;}
    }

    //public class PackContentSpec
    //{
    //    public int Count { get; set; }
    //    public string OverallRarity { get; set; }
    //}

    public class Miscellaneous : Product
    {

    }

    public class SealedDeck : Product
    {

    }

    public class RandomPickParameters
    {
        public List<string>? OverallRarities { get; set; }
        public string? Side { get; set; }
    }
}
