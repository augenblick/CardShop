use indicatif::{ProgressBar, ProgressStyle};
use rand::seq::SliceRandom;
use rand::thread_rng;
mod cards;

use std::collections::HashMap;

use crate::cards::pr::PremiereCards;

fn main() {
    // get file path as a command line argument
    let file_path = std::env::args().nth(1).expect("Please provide a file path");

    let mut rng = thread_rng();

    let count = 1_000_000;

    let pb = ProgressBar::new(count);
    let pb_style = ProgressStyle::default_bar()
        .template("[{elapsed_precise}] {bar:40.cyan/blue} {pos:>7}/{len:7} {msg}")
        .unwrap()
        .progress_chars("##-");

    pb.set_style(pb_style);

    // load the cards JSON file into a Cards object
    let set = load_card_set(&file_path);
    // // log the cards object to stdout
    // println!("cards: {:#?}", set);

    println!("card set: {}", set.name.unwrap());
    println!("number of cards: {}", set.size.unwrap());
    println!("size of cards vec {}", set.cards.as_ref().unwrap().len());

    let mut card_counts: HashMap<String, CardCount> = HashMap::new();

    println!("drawing {}", &count);
    for i in 0..count {
        // get random card from cards vec
        let card = set.cards.as_ref().unwrap().choose(&mut rng).unwrap();

        pb.set_position(i);
        pb.set_message(format!("Processing item {} {}", i, card.name.as_ref().unwrap()));

        // if card_counts contains card code (as key), increment count
        // otherwise, add card code to card_counts and set count to 1
        if card_counts.contains_key(card.code.as_ref().unwrap()) {
            card_counts
                .get_mut(card.code.as_ref().unwrap())
                .unwrap()
                .count += 1;
        } else {
            let card_name = card.name.as_ref().unwrap().to_string();
            let rarity = card.rarity_code.as_ref().unwrap().to_owned();
            card_counts.insert(
                card.code.as_ref().unwrap().to_string(),
                CardCount {
                    card_name,
                    rarity,
                    count: 1,
                },
            );
        }
    }
    pb.finish_with_message("Done!");

    // print number of unique cards
    println!("number of unique cards: {}", card_counts.len());
    println!("card_counts: {:#?}", card_counts);

}

/// Function to load the cards JSON file into a Cards object at runtime.
/// `file_path` is the path to the cards JSON file.
fn load_card_set(file_path: &str) -> PremiereCards {
    // do not use include_str!() here, it will not work in a binary crate
    let json = std::fs::read_to_string(file_path).expect("Failed to read file");

    serde_json::from_str(&json).unwrap()
}
// pub fn load_card_set() -> cards::Cards {
//     // load JSON from file
//     let json = include_str!("..\\..\\..\\CardShop\\Repositories\\Data\\CardSets\\pr.json");
//     // parse JSON into Cards object
//     serde_json::from_str(json).unwrap()
// }

#[allow(dead_code)]
#[derive(Debug)]
struct CardCount {
    pub card_name: String,
    pub rarity: cards::pr::Rarity,
    pub count: u64,
}
