using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CardShop.Models
{
    public partial class Card : Product
    {
        [JsonProperty("ability", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ability { get; set; }

        [JsonProperty("armor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Armor { get; set; }

        [JsonProperty("characteristics", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string Characteristics { get; set; }

        [JsonProperty("clone_army", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CloneArmy { get; set; }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public new string Code { get; set; }

        [JsonProperty("dark_side_icons", NullValueHandling = NullValueHandling.Ignore)]
        public long? DarkSideIcons { get; set; }

        [JsonProperty("dark_side_text", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string DarkSideText { get; set; }

        [JsonProperty("defense_value", NullValueHandling = NullValueHandling.Ignore)]
        public long? DefenseValue { get; set; }

        [JsonProperty("defense_value_name", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
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
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string ForceAptitude { get; set; }

        [JsonProperty("forfeit")]
        public long? Forfeit { get; set; }

        [JsonProperty("gametext", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
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
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string LightSideText { get; set; }

        [JsonProperty("lore", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string Lore { get; set; }

        [JsonProperty("maneuver", NullValueHandling = NullValueHandling.Ignore)]
        public long? Maneuver { get; set; }

        [JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mobile { get; set; }

        [JsonProperty("model_type", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string ModelType { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public new string Name { get; set; }

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
        [JsonConverter(typeof(FluffyMinMaxLengthCheckConverter))]
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
        [JsonConverter(typeof(PurpleMinMaxLengthCheckConverter))]
        public string Uniqueness { get; set; }

        [JsonProperty("warrior", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Warrior { get; set; }

    }

    public enum RarityCode { C, C1, C2, C3, F, Pm, Pm2, Pm3, Pm5, Pv, R, R1, R2, U, U1, U2, Ur, Xr };

    public enum SideCode { Dark, Light };

    public enum SubtypeCode { Alien, AlienImperial, AlienRebel, AlienResistance, Artillery, Automated, Capital, Character, Combat, Creature, DarkJediMaster, DeathStar, DeathStarIi, Droid, FirstOrder, Immediate, Imperial, Jedi, JediMaster, JediMasterImperial, Lost, LostOrStarting, Mobile, Political, Rebel, RebelRepublic, Republic, Resistance, Sector, Shuttle, Site, Sith, Squadron, Starfighter, Starship, Starting, System, Transport, Used, UsedOrLost, UsedOrStarting, Utinni, Vehicle };

    public enum TypeCode { AdmiralsOrder, Character, Creature, DefensiveShield, Device, Effect, EpicEvent, Interrupt, JediTest, Location, Objective, Podracer, Starship, Vehicle, Weapon };

    public partial class Card
    {
        public static Card FromJson(string json) => JsonConvert.DeserializeObject<Card>(json, CardShop.Models.Converter.Settings);
    }



    public static class Serialize
    {
        public static string ToJson(this Card self) => JsonConvert.SerializeObject(self, CardShop.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RarityCodeConverter.Singleton,
                SideCodeConverter.Singleton,
                SubtypeCodeConverter.Singleton,
                TypeCodeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class PurpleMinMaxLengthCheckConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(string);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader);
            if (value.Length >= 0)
            {
                return value;
            }
            throw new Exception("Cannot unmarshal type string");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (string)untypedValue;
            if (value.Length >= 0)
            {
                serializer.Serialize(writer, value);
                return;
            }
            throw new Exception("Cannot marshal type string");
        }

        public static readonly PurpleMinMaxLengthCheckConverter Singleton = new PurpleMinMaxLengthCheckConverter();
    }

    internal class RarityCodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(RarityCode) || t == typeof(RarityCode?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "C":
                    return RarityCode.C;
                case "C1":
                    return RarityCode.C1;
                case "C2":
                    return RarityCode.C2;
                case "C3":
                    return RarityCode.C3;
                case "F":
                    return RarityCode.F;
                case "PM":
                    return RarityCode.Pm;
                case "PM2":
                    return RarityCode.Pm2;
                case "PM3":
                    return RarityCode.Pm3;
                case "PM5":
                    return RarityCode.Pm5;
                case "PV":
                    return RarityCode.Pv;
                case "R":
                    return RarityCode.R;
                case "R1":
                    return RarityCode.R1;
                case "R2":
                    return RarityCode.R2;
                case "U":
                    return RarityCode.U;
                case "U1":
                    return RarityCode.U1;
                case "U2":
                    return RarityCode.U2;
                case "UR":
                    return RarityCode.Ur;
                case "XR":
                    return RarityCode.Xr;
            }
            throw new Exception("Cannot unmarshal type RarityCode");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (RarityCode)untypedValue;
            switch (value)
            {
                case RarityCode.C:
                    serializer.Serialize(writer, "C");
                    return;
                case RarityCode.C1:
                    serializer.Serialize(writer, "C1");
                    return;
                case RarityCode.C2:
                    serializer.Serialize(writer, "C2");
                    return;
                case RarityCode.C3:
                    serializer.Serialize(writer, "C3");
                    return;
                case RarityCode.F:
                    serializer.Serialize(writer, "F");
                    return;
                case RarityCode.Pm:
                    serializer.Serialize(writer, "PM");
                    return;
                case RarityCode.Pm2:
                    serializer.Serialize(writer, "PM2");
                    return;
                case RarityCode.Pm3:
                    serializer.Serialize(writer, "PM3");
                    return;
                case RarityCode.Pm5:
                    serializer.Serialize(writer, "PM5");
                    return;
                case RarityCode.Pv:
                    serializer.Serialize(writer, "PV");
                    return;
                case RarityCode.R:
                    serializer.Serialize(writer, "R");
                    return;
                case RarityCode.R1:
                    serializer.Serialize(writer, "R1");
                    return;
                case RarityCode.R2:
                    serializer.Serialize(writer, "R2");
                    return;
                case RarityCode.U:
                    serializer.Serialize(writer, "U");
                    return;
                case RarityCode.U1:
                    serializer.Serialize(writer, "U1");
                    return;
                case RarityCode.U2:
                    serializer.Serialize(writer, "U2");
                    return;
                case RarityCode.Ur:
                    serializer.Serialize(writer, "UR");
                    return;
                case RarityCode.Xr:
                    serializer.Serialize(writer, "XR");
                    return;
            }
            throw new Exception("Cannot marshal type RarityCode");
        }

        public static readonly RarityCodeConverter Singleton = new RarityCodeConverter();
    }

    internal class FluffyMinMaxLengthCheckConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(string);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader);
            if (value.Length >= 2 && value.Length <= 5)
            {
                return value;
            }
            throw new Exception("Cannot unmarshal type string");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (string)untypedValue;
            if (value.Length >= 2 && value.Length <= 5)
            {
                serializer.Serialize(writer, value);
                return;
            }
            throw new Exception("Cannot marshal type string");
        }

        public static readonly FluffyMinMaxLengthCheckConverter Singleton = new FluffyMinMaxLengthCheckConverter();
    }

    internal class SideCodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SideCode) || t == typeof(SideCode?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "dark":
                    return SideCode.Dark;
                case "light":
                    return SideCode.Light;
            }
            throw new Exception("Cannot unmarshal type SideCode");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SideCode)untypedValue;
            switch (value)
            {
                case SideCode.Dark:
                    serializer.Serialize(writer, "dark");
                    return;
                case SideCode.Light:
                    serializer.Serialize(writer, "light");
                    return;
            }
            throw new Exception("Cannot marshal type SideCode");
        }

        public static readonly SideCodeConverter Singleton = new SideCodeConverter();
    }

    internal class SubtypeCodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SubtypeCode) || t == typeof(SubtypeCode?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "alien":
                    return SubtypeCode.Alien;
                case "alien-imperial":
                    return SubtypeCode.AlienImperial;
                case "alien-rebel":
                    return SubtypeCode.AlienRebel;
                case "alien-resistance":
                    return SubtypeCode.AlienResistance;
                case "artillery":
                    return SubtypeCode.Artillery;
                case "automated":
                    return SubtypeCode.Automated;
                case "capital":
                    return SubtypeCode.Capital;
                case "character":
                    return SubtypeCode.Character;
                case "combat":
                    return SubtypeCode.Combat;
                case "creature":
                    return SubtypeCode.Creature;
                case "dark-jedi-master":
                    return SubtypeCode.DarkJediMaster;
                case "death-star":
                    return SubtypeCode.DeathStar;
                case "death-star-II":
                    return SubtypeCode.DeathStarIi;
                case "droid":
                    return SubtypeCode.Droid;
                case "first-order":
                    return SubtypeCode.FirstOrder;
                case "immediate":
                    return SubtypeCode.Immediate;
                case "imperial":
                    return SubtypeCode.Imperial;
                case "jedi":
                    return SubtypeCode.Jedi;
                case "jedi-master":
                    return SubtypeCode.JediMaster;
                case "jedi-master-imperial":
                    return SubtypeCode.JediMasterImperial;
                case "lost":
                    return SubtypeCode.Lost;
                case "lost-or-starting":
                    return SubtypeCode.LostOrStarting;
                case "mobile":
                    return SubtypeCode.Mobile;
                case "political":
                    return SubtypeCode.Political;
                case "rebel":
                    return SubtypeCode.Rebel;
                case "rebel-republic":
                    return SubtypeCode.RebelRepublic;
                case "republic":
                    return SubtypeCode.Republic;
                case "resistance":
                    return SubtypeCode.Resistance;
                case "sector":
                    return SubtypeCode.Sector;
                case "shuttle":
                    return SubtypeCode.Shuttle;
                case "site":
                    return SubtypeCode.Site;
                case "sith":
                    return SubtypeCode.Sith;
                case "squadron":
                    return SubtypeCode.Squadron;
                case "starfighter":
                    return SubtypeCode.Starfighter;
                case "starship":
                    return SubtypeCode.Starship;
                case "starting":
                    return SubtypeCode.Starting;
                case "system":
                    return SubtypeCode.System;
                case "transport":
                    return SubtypeCode.Transport;
                case "used":
                    return SubtypeCode.Used;
                case "used-or-lost":
                    return SubtypeCode.UsedOrLost;
                case "used-or-starting":
                    return SubtypeCode.UsedOrStarting;
                case "utinni":
                    return SubtypeCode.Utinni;
                case "vehicle":
                    return SubtypeCode.Vehicle;
            }
            throw new Exception("Cannot unmarshal type SubtypeCode");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SubtypeCode)untypedValue;
            switch (value)
            {
                case SubtypeCode.Alien:
                    serializer.Serialize(writer, "alien");
                    return;
                case SubtypeCode.AlienImperial:
                    serializer.Serialize(writer, "alien-imperial");
                    return;
                case SubtypeCode.AlienRebel:
                    serializer.Serialize(writer, "alien-rebel");
                    return;
                case SubtypeCode.AlienResistance:
                    serializer.Serialize(writer, "alien-resistance");
                    return;
                case SubtypeCode.Artillery:
                    serializer.Serialize(writer, "artillery");
                    return;
                case SubtypeCode.Automated:
                    serializer.Serialize(writer, "automated");
                    return;
                case SubtypeCode.Capital:
                    serializer.Serialize(writer, "capital");
                    return;
                case SubtypeCode.Character:
                    serializer.Serialize(writer, "character");
                    return;
                case SubtypeCode.Combat:
                    serializer.Serialize(writer, "combat");
                    return;
                case SubtypeCode.Creature:
                    serializer.Serialize(writer, "creature");
                    return;
                case SubtypeCode.DarkJediMaster:
                    serializer.Serialize(writer, "dark-jedi-master");
                    return;
                case SubtypeCode.DeathStar:
                    serializer.Serialize(writer, "death-star");
                    return;
                case SubtypeCode.DeathStarIi:
                    serializer.Serialize(writer, "death-star-II");
                    return;
                case SubtypeCode.Droid:
                    serializer.Serialize(writer, "droid");
                    return;
                case SubtypeCode.FirstOrder:
                    serializer.Serialize(writer, "first-order");
                    return;
                case SubtypeCode.Immediate:
                    serializer.Serialize(writer, "immediate");
                    return;
                case SubtypeCode.Imperial:
                    serializer.Serialize(writer, "imperial");
                    return;
                case SubtypeCode.Jedi:
                    serializer.Serialize(writer, "jedi");
                    return;
                case SubtypeCode.JediMaster:
                    serializer.Serialize(writer, "jedi-master");
                    return;
                case SubtypeCode.JediMasterImperial:
                    serializer.Serialize(writer, "jedi-master-imperial");
                    return;
                case SubtypeCode.Lost:
                    serializer.Serialize(writer, "lost");
                    return;
                case SubtypeCode.LostOrStarting:
                    serializer.Serialize(writer, "lost-or-starting");
                    return;
                case SubtypeCode.Mobile:
                    serializer.Serialize(writer, "mobile");
                    return;
                case SubtypeCode.Political:
                    serializer.Serialize(writer, "political");
                    return;
                case SubtypeCode.Rebel:
                    serializer.Serialize(writer, "rebel");
                    return;
                case SubtypeCode.RebelRepublic:
                    serializer.Serialize(writer, "rebel-republic");
                    return;
                case SubtypeCode.Republic:
                    serializer.Serialize(writer, "republic");
                    return;
                case SubtypeCode.Resistance:
                    serializer.Serialize(writer, "resistance");
                    return;
                case SubtypeCode.Sector:
                    serializer.Serialize(writer, "sector");
                    return;
                case SubtypeCode.Shuttle:
                    serializer.Serialize(writer, "shuttle");
                    return;
                case SubtypeCode.Site:
                    serializer.Serialize(writer, "site");
                    return;
                case SubtypeCode.Sith:
                    serializer.Serialize(writer, "sith");
                    return;
                case SubtypeCode.Squadron:
                    serializer.Serialize(writer, "squadron");
                    return;
                case SubtypeCode.Starfighter:
                    serializer.Serialize(writer, "starfighter");
                    return;
                case SubtypeCode.Starship:
                    serializer.Serialize(writer, "starship");
                    return;
                case SubtypeCode.Starting:
                    serializer.Serialize(writer, "starting");
                    return;
                case SubtypeCode.System:
                    serializer.Serialize(writer, "system");
                    return;
                case SubtypeCode.Transport:
                    serializer.Serialize(writer, "transport");
                    return;
                case SubtypeCode.Used:
                    serializer.Serialize(writer, "used");
                    return;
                case SubtypeCode.UsedOrLost:
                    serializer.Serialize(writer, "used-or-lost");
                    return;
                case SubtypeCode.UsedOrStarting:
                    serializer.Serialize(writer, "used-or-starting");
                    return;
                case SubtypeCode.Utinni:
                    serializer.Serialize(writer, "utinni");
                    return;
                case SubtypeCode.Vehicle:
                    serializer.Serialize(writer, "vehicle");
                    return;
            }
            throw new Exception("Cannot marshal type SubtypeCode");
        }

        public static readonly SubtypeCodeConverter Singleton = new SubtypeCodeConverter();
    }

    internal class TypeCodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeCode) || t == typeof(TypeCode?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "admirals-order":
                    return TypeCode.AdmiralsOrder;
                case "character":
                    return TypeCode.Character;
                case "creature":
                    return TypeCode.Creature;
                case "defensive-shield":
                    return TypeCode.DefensiveShield;
                case "device":
                    return TypeCode.Device;
                case "effect":
                    return TypeCode.Effect;
                case "epic-event":
                    return TypeCode.EpicEvent;
                case "interrupt":
                    return TypeCode.Interrupt;
                case "jedi-test":
                    return TypeCode.JediTest;
                case "location":
                    return TypeCode.Location;
                case "objective":
                    return TypeCode.Objective;
                case "podracer":
                    return TypeCode.Podracer;
                case "starship":
                    return TypeCode.Starship;
                case "vehicle":
                    return TypeCode.Vehicle;
                case "weapon":
                    return TypeCode.Weapon;
            }
            throw new Exception("Cannot unmarshal type TypeCode");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeCode)untypedValue;
            switch (value)
            {
                case TypeCode.AdmiralsOrder:
                    serializer.Serialize(writer, "admirals-order");
                    return;
                case TypeCode.Character:
                    serializer.Serialize(writer, "character");
                    return;
                case TypeCode.Creature:
                    serializer.Serialize(writer, "creature");
                    return;
                case TypeCode.DefensiveShield:
                    serializer.Serialize(writer, "defensive-shield");
                    return;
                case TypeCode.Device:
                    serializer.Serialize(writer, "device");
                    return;
                case TypeCode.Effect:
                    serializer.Serialize(writer, "effect");
                    return;
                case TypeCode.EpicEvent:
                    serializer.Serialize(writer, "epic-event");
                    return;
                case TypeCode.Interrupt:
                    serializer.Serialize(writer, "interrupt");
                    return;
                case TypeCode.JediTest:
                    serializer.Serialize(writer, "jedi-test");
                    return;
                case TypeCode.Location:
                    serializer.Serialize(writer, "location");
                    return;
                case TypeCode.Objective:
                    serializer.Serialize(writer, "objective");
                    return;
                case TypeCode.Podracer:
                    serializer.Serialize(writer, "podracer");
                    return;
                case TypeCode.Starship:
                    serializer.Serialize(writer, "starship");
                    return;
                case TypeCode.Vehicle:
                    serializer.Serialize(writer, "vehicle");
                    return;
                case TypeCode.Weapon:
                    serializer.Serialize(writer, "weapon");
                    return;
            }
            throw new Exception("Cannot marshal type TypeCode");
        }

        public static readonly TypeCodeConverter Singleton = new TypeCodeConverter();
    }
}
