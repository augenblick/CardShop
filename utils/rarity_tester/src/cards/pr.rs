/// For the card set Premiere

use serde::{Serialize, Deserialize};

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct PremiereCards {
    pub code: Option<Code>,
    pub cycle_code: Option<String>,
    pub date_release: Option<String>,
    pub name: Option<String>,
    pub position: Option<i64>,
    pub size: Option<i64>,
    pub products: Option<Vec<Product>>,
    #[serde(rename = "cardRarityMap")]
    pub card_rarity_map: Option<Vec<CardRarityMap>>,
    pub cards: Option<Vec<Card>>,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct CardRarityMap {
    pub rarity: Option<Rarity>,
    pub count: Option<i64>,
    pub overall_rarity: Option<OverallRarity>,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, Eq, Hash)]
pub enum Rarity {
    C1,
    C2,
    C3,
    R1,
    R2,
    U1,
    U2,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, Eq, Hash)]
pub enum OverallRarity {
    C,
    U,
    R,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Card {
    pub ability: Option<i64>,
    pub armor: Option<i64>,
    pub characteristics: Option<String>,
    pub clone_army: Option<bool>,
    pub code: Option<String>,
    pub deploy: Option<i64>,
    pub destiny: Option<i64>,
    pub episode_1: Option<bool>,
    pub episode_7: Option<bool>,
    pub force_aptitude: Option<ForceAptitude>,
    pub forfeit: Option<i64>,
    pub gametext: Option<String>,
    pub has_errata: Option<bool>,
    pub image_url: Option<String>,
    pub lore: Option<String>,
    pub maneuver: Option<i64>,
    pub model_type: Option<String>,
    pub name: Option<String>,
    pub nav_computer: Option<bool>,
    pub permanent_weapon: Option<bool>,
    pub pilot: Option<bool>,
    pub politics: Option<i64>,
    pub position: Option<i64>,
    pub power: Option<i64>,
    pub presence: Option<bool>,
    pub rarity_code: Option<Rarity>,
    pub republic: Option<bool>,
    pub separatist: Option<bool>,
    pub set_code: Option<Code>,
    pub side_code: Option<SideCode>,
    pub subtype_code: Option<SubtypeCode>,
    pub type_code: Option<TypeCode>,
    pub uniqueness: Option<Uniqueness>,
    pub warrior: Option<bool>,
    pub grabber: Option<bool>,
    pub dark_side_icons: Option<i64>,
    pub dark_side_text: Option<String>,
    pub light_side_icons: Option<i64>,
    pub light_side_text: Option<String>,
    pub mobile: Option<bool>,
    pub planet: Option<bool>,
    pub scomp_link: Option<bool>,
    pub site_creature: Option<bool>,
    pub site_exterior: Option<bool>,
    pub site_interior: Option<bool>,
    pub site_starship: Option<bool>,
    pub site_underground: Option<bool>,
    pub site_underwater: Option<bool>,
    pub site_vehicle: Option<bool>,
    pub space: Option<bool>,
    pub system_parsec: Option<i64>,
    pub first_order: Option<bool>,
    pub hyperspeed: Option<i64>,
    pub independent: Option<bool>,
    pub resistance: Option<bool>,
    pub trade_federation: Option<bool>,
    pub landspeed: Option<i64>,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub enum ForceAptitude {
    #[serde(rename = "Dark Jedi")]
    DarkJedi,
    #[serde(rename = "")]
    Empty,
    #[serde(rename = "Force-Attuned")]
    ForceAttuned,
    #[serde(rename = "Force-Sensitive")]
    ForceSensitive,
    #[serde(rename = "Jedi Knight")]
    JediKnight,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum Code {
    Pr,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum SideCode {
    Dark,
    Light,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum SubtypeCode {
    Alien,
    Automated,
    Capital,
    Character,
    Creature,
    Droid,
    Imperial,
    Lost,
    Rebel,
    Site,
    Starfighter,
    Starship,
    System,
    Transport,
    Used,
    Utinni,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum TypeCode {
    Character,
    Device,
    Effect,
    Interrupt,
    Location,
    Starship,
    Vehicle,
    Weapon,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub enum Uniqueness {
    #[serde(rename = "")]
    Empty,
    #[serde(rename = "**")]
    TwoStars,
    #[serde(rename = "***")]
    ThreeStars,
    #[serde(rename = "*")]
    OneStar,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Product {
    pub code: Option<String>,
    pub name: Option<String>,
    pub product_type: Option<String>,
    pub contents: Option<Vec<Content>>,
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Content {
    pub code: Option<String>,
    pub count: Option<i64>,
}
