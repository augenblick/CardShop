using Newtonsoft.Json;

namespace CardShop.Models
{
    public class Card : Product
    {
        [JsonProperty("ability", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ability { get; set; }

        [JsonProperty("armor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Armor { get; set; }

        [JsonProperty("characteristics", NullValueHandling = NullValueHandling.Ignore)]
        public string Characteristics { get; set; }

        [JsonProperty("clone_army", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CloneArmy { get; set; }

        //[JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        //public new string Code { get; set; }

        [JsonProperty("dark_side_icons", NullValueHandling = NullValueHandling.Ignore)]
        public long? DarkSideIcons { get; set; }

        [JsonProperty("dark_side_text", NullValueHandling = NullValueHandling.Ignore)]
        public string DarkSideText { get; set; }

        [JsonProperty("defense_value", NullValueHandling = NullValueHandling.Ignore)]
        public long? DefenseValue { get; set; }

        [JsonProperty("defense_value_name", NullValueHandling = NullValueHandling.Ignore)]
        public string DefenseValueName { get; set; }

        [JsonProperty("deploy")]
        public long? Deploy { get; set; }

        [JsonProperty("destiny")]
        public long? Destiny { get; set; }

        [JsonProperty("episode_1", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Episode1 { get; set; }

        [JsonProperty("episode_7", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Episode7 { get; set; }

        [JsonProperty("ferocity")]
        public long? Ferocity { get; set; }

        [JsonProperty("first_order", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FirstOrder { get; set; }

        [JsonProperty("force_aptitude", NullValueHandling = NullValueHandling.Ignore)]
        public string ForceAptitude { get; set; }

        [JsonProperty("forfeit")]
        public long? Forfeit { get; set; }

        [JsonProperty("gametext", NullValueHandling = NullValueHandling.Ignore)]
        public string Gametext { get; set; }

        [JsonProperty("grabber", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Grabber { get; set; }

        [JsonProperty("has_errata", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasErrata { get; set; }

        [JsonProperty("hyperspeed", NullValueHandling = NullValueHandling.Ignore)]
        public long? Hyperspeed { get; set; }

        //[JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
        //[JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        //public string ImageUrl { get; set; }

        //[JsonProperty("image_url_2", NullValueHandling = NullValueHandling.Ignore)]
        //[JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        //public string ImageUrl2 { get; set; }

        [JsonProperty("independent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Independent { get; set; }

        [JsonProperty("is_horizontal", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsHorizontal { get; set; }

        [JsonProperty("landspeed")]
        public long? Landspeed { get; set; }

        [JsonProperty("light_side_icons", NullValueHandling = NullValueHandling.Ignore)]
        public long? LightSideIcons { get; set; }

        [JsonProperty("light_side_text", NullValueHandling = NullValueHandling.Ignore)]
        public string LightSideText { get; set; }

        [JsonProperty("lore", NullValueHandling = NullValueHandling.Ignore)]
        public string Lore { get; set; }

        [JsonProperty("maneuver", NullValueHandling = NullValueHandling.Ignore)]
        public long? Maneuver { get; set; }

        [JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mobile { get; set; }

        [JsonProperty("model_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ModelType { get; set; }

        //[DataMember(Name = "name")]
        //[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        //public new string Name { get; set; }

        [JsonProperty("nav_computer", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NavComputer { get; set; }

        [JsonProperty("permanent_weapon", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PermanentWeapon { get; set; }

        [JsonProperty("pilot", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Pilot { get; set; }

        [JsonProperty("planet", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Planet { get; set; }

        [JsonProperty("politics", NullValueHandling = NullValueHandling.Ignore)]
        public long? Politics { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("power")]
        public long? Power { get; set; }

        [JsonProperty("presence", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Presence { get; set; }

        [JsonProperty("rarity_code", NullValueHandling = NullValueHandling.Ignore)]
        public string RarityCode { get; set; }

        [JsonProperty("republic", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Republic { get; set; }

        [JsonProperty("resistance", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Resistance { get; set; }

        [JsonProperty("scomp_link", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ScompLink { get; set; }

        [JsonProperty("selective", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Selective { get; set; }

        [JsonProperty("separatist", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Separatist { get; set; }

        [JsonProperty("set_code", NullValueHandling = NullValueHandling.Ignore)]
        public string SetCode { get; set; }

        [JsonProperty("side_code", NullValueHandling = NullValueHandling.Ignore)]
        public string SideCode { get; set; }

        [JsonProperty("site_creature", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteCreature { get; set; }

        [JsonProperty("site_exterior", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteExterior { get; set; }

        [JsonProperty("site_interior", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteInterior { get; set; }

        [JsonProperty("site_starship", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteStarship { get; set; }

        [JsonProperty("site_underground", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteUnderground { get; set; }

        [JsonProperty("site_underwater", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteUnderwater { get; set; }

        [JsonProperty("site_vehicle", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SiteVehicle { get; set; }

        [JsonProperty("space", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Space { get; set; }

        [JsonProperty("subtype_code", NullValueHandling = NullValueHandling.Ignore)]
        public string SubtypeCode { get; set; }

        [JsonProperty("system_parsec")]
        public long? SystemParsec { get; set; }

        [JsonProperty("trade_federation", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TradeFederation { get; set; }

        [JsonProperty("type_code", NullValueHandling = NullValueHandling.Ignore)]
        public string TypeCode { get; set; }

        [JsonProperty("uniqueness", NullValueHandling = NullValueHandling.Ignore)]
        public string Uniqueness { get; set; }

        [JsonProperty("warrior", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Warrior { get; set; }

        [JsonProperty("is_foil", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFoil { get; set; } = false;

        [JsonProperty("is_japanese", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsJapanese { get; set; } = false;

        public override decimal CostPer { get; set; } = 0M;

        public Card()
        {
            ProductType = ProductType.Card;
        }

        //public static Card FromJson(string json) => JsonConvert.DeserializeObject<Card>(json, Converter.Settings);
    }


    
}
