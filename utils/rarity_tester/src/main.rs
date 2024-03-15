use indicatif::{ProgressBar, ProgressStyle};
use rand::distributions::WeightedIndex;
use rand::prelude::*;
use std::collections::HashMap;

mod cards;
use crate::cards::pr::{OverallRarity, PremiereCards, Rarity};

fn main() {
    // get file path as a command line argument
    let file_path = std::env::args().nth(1).expect("Please provide a file path");

    let count = 1_000_000_000;

    // set up progress bar
    let pb = ProgressBar::new(count);
    let pb_style = ProgressStyle::default_bar()
        .template("[{elapsed_precise}] {bar:40.cyan/blue} {pos:>7}/{len:7} {msg}")
        .unwrap()
        .progress_chars("##-");
    pb.set_style(pb_style);

    // load the cards JSON file into a Cards object
    let set = load_card_set(&file_path);
    println!("card set: {}", set.name.as_ref().unwrap());
    println!("number of cards: {}", set.size.unwrap());
    println!("size of cards vec {}", set.cards.as_ref().unwrap().len());

    let commons = set
        .cards
        .as_ref()
        .unwrap()
        .iter()
        .filter(|card| {
            matches!(
                card.rarity_code.as_ref().unwrap(),
                Rarity::C1 | Rarity::C2 | Rarity::C3
            )
        })
        .collect::<Vec<_>>();

    let uncommons = set
        .cards
        .as_ref()
        .unwrap()
        .iter()
        .filter(|card| matches!(card.rarity_code.as_ref().unwrap(), Rarity::U1 | Rarity::U2))
        .collect::<Vec<_>>();

    let rares = set
        .cards
        .as_ref()
        .unwrap()
        .iter()
        .filter(|card| matches!(card.rarity_code.as_ref().unwrap(), Rarity::R1 | Rarity::R2))
        .collect::<Vec<_>>();

    println!("number of commons: {}", commons.len());
    println!("number of uncommons: {}", uncommons.len());
    println!("number of rares: {}", rares.len());
    println!("total {}", commons.len() + uncommons.len() + rares.len());

    fn open_pack(set: &PremiereCards) -> Vec<cards::pr::Card> {
        let pack = set
            .products
            .as_ref()
            .unwrap()
            .iter()
            .find(|p| p.product_type.as_ref().unwrap() == "BoosterPack")
            .unwrap();

        let num_commons_to_pull = pack
            .contents
            .as_ref()
            .unwrap()
            .iter()
            .find(|c| c.code.as_ref().unwrap() == "C")
            .unwrap()
            .count
            .unwrap();

        let num_uncommons_to_pull = pack
            .contents
            .as_ref()
            .unwrap()
            .iter()
            .find(|c| c.code.as_ref().unwrap() == "U")
            .unwrap()
            .count
            .unwrap();

        let num_rares_to_pull = pack
            .contents
            .as_ref()
            .unwrap()
            .iter()
            .find(|c| c.code.as_ref().unwrap() == "R")
            .unwrap()
            .count
            .unwrap();

        println!("num_commons_to_pull: {}", num_commons_to_pull);
        println!("num_uncommons_to_pull: {}", num_uncommons_to_pull);
        println!("num_rares_to_pull: {}", num_rares_to_pull);

        let rarity_pool: HashMap<OverallRarity, Vec<(Rarity, u8)>> = {
            vec![
                (
                    OverallRarity::C,
                    vec![(Rarity::C1, 1), (Rarity::C2, 2), (Rarity::C3, 3)],
                ),
                (OverallRarity::U, vec![(Rarity::U1, 1), (Rarity::U2, 2)]),
                (OverallRarity::R, vec![(Rarity::R1, 1), (Rarity::R2, 2)]),
            ]
        }
        .into_iter()
        .collect();

        println!("rarity_pool: {:?}", rarity_pool);

        todo!();
    }

    open_pack(&set);
}

/// Function to load the cards JSON file into a Cards object at runtime.
/// `file_path` is the path to the cards JSON file.
fn load_card_set(file_path: &str) -> PremiereCards {
    // do not use include_str!() here, it will not work in a binary crate
    let json = std::fs::read_to_string(file_path).expect("Failed to read file");

    serde_json::from_str(&json).unwrap()
}

#[allow(dead_code)]
#[derive(Debug)]
struct CardCount {
    pub card_name: String,
    pub rarity: cards::pr::Rarity,
    pub count: u64,
}
