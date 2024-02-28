using CardShop.Constants;
using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using Dapper;
using Microsoft.Identity.Client;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardShop.Logic
{
    // TODO: a lot of room for optimization throughout

    public class CardProductBuilder : ICardProductBuilder
    {
        private CardSet _premiereSet = new CardSet(CardSetConstants.Premiere);
        private CardSet _newHopeSet = new CardSet(CardSetConstants.NewHope);
        private CardSet _hothSet = new CardSet(CardSetConstants.Hoth);
        private CardSet _dagobahSet = new CardSet(CardSetConstants.Dagobah);
        private CardSet _cloudCitySet = new CardSet(CardSetConstants.CloudCity);
        private CardSet _specialEditionSet = new CardSet(CardSetConstants.SpecialEdition);

        private IEnumerable<Card> _cards;

        private readonly Random _randomizer = new Random();

        private readonly ICardTestRepository _cardTestRepository;

        public CardProductBuilder(ICardTestRepository cardTestRepository)
        {
            _cardTestRepository = cardTestRepository;

            _cards = _cardTestRepository.GetCardSets();

            _premiereSet.FillSet(_cards.Where(x => x.Set == CardSetConstants.Premiere));
            _newHopeSet.FillSet(_cards.Where(x => x.Set == CardSetConstants.NewHope));
            _hothSet.FillSet(_cards.Where(x => x.Set == CardSetConstants.Hoth));
            _dagobahSet.FillSet(_cards.Where(x => x.Set == CardSetConstants.Dagobah));
            _cloudCitySet.FillSet(_cards.Where(x => x.Set == CardSetConstants.CloudCity));
            _specialEditionSet.FillSet(_cards.Where(x => x.Set == CardSetConstants.SpecialEdition));
        }

        public IEnumerable<CardPack> GetCardPacks(int count, string cardSetName)
        {
            
            var cardPacks = new List<CardPack>();
            if (count < 1) { return cardPacks; }

            for (int i = 0; i < count; i++)
            {
                cardPacks.Add(GetCardPack(cardSetName));
            }

            return cardPacks;
        }

        private CardPack GetCardPack(string cardSetName)
        {
            var cardPack = new CardPack();

            if (cardSetName.ToLower().Contains("premiere")){
                cardSetName = CardSetConstants.Premiere;
            }
            else if (cardSetName.ToLower().Contains("hoth")){
                cardSetName = CardSetConstants.Hoth;
            }
            else if (cardSetName.ToLower().Contains("dagobah")){
                cardSetName = CardSetConstants.Dagobah;
            }
            else if (cardSetName.ToLower().Contains("cloud")){
                cardSetName = CardSetConstants.CloudCity;
            }
            else if (cardSetName.ToLower().Contains("special")){
                cardSetName = CardSetConstants.SpecialEdition;
            }
            else if (cardSetName.ToLower().Contains("hope")){
                cardSetName = CardSetConstants.NewHope;
            }

            if (cardSetName == CardSetConstants.Premiere || cardSetName == CardSetConstants.Hoth || cardSetName == CardSetConstants.NewHope) 
            { 
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "R"));

                cardPack.CardSet = cardSetName;
            }
            if (cardSetName == CardSetConstants.CloudCity || cardSetName == CardSetConstants.Dagobah || cardSetName == CardSetConstants.SpecialEdition)
            {
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "C"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "U"));
                cardPack.CardList.Add(ChooseCardAndRemoveFromSet(cardSetName, "R"));

                cardPack.CardSet = cardSetName;
            }
                
            return cardPack;
        }

        private Card ChooseCardAndRemoveFromSet(string cardSetName, string overallRarity) 
        {
            var selectedSet = new CardSet();

            switch (cardSetName)
            {
                case CardSetConstants.Premiere:
                    selectedSet = _premiereSet;
                    break;
                case CardSetConstants.NewHope:
                    selectedSet = _newHopeSet;
                    break;
                case CardSetConstants.Hoth:
                    selectedSet = _hothSet;
                    break;
                case CardSetConstants.Dagobah:
                    selectedSet = _dagobahSet;
                    break;
                case CardSetConstants.CloudCity:
                    selectedSet = _cloudCitySet;
                    break;
                case CardSetConstants.SpecialEdition:
                    selectedSet = _specialEditionSet;
                    break;
                default:
                    break;
            }

            if (string.IsNullOrWhiteSpace(selectedSet.CardSetName))
            {
                // TODO: log error
                return null;
            }

            // TODO: remove this
            // This is a hacky way of excluding the chosen card from being picked again on the next go-round
            var exclusionString = "@#$%";

            if (!selectedSet.CardSetContents.Any(x => x.Rarity.Contains(overallRarity) && !x.Name.Contains(exclusionString)))
            {
                // we don't have the cards we need, so replenish the set
                selectedSet.FillSet(_cards.Where(x => x.Set == selectedSet.CardSetName));
            }

            // TODO: Make this support all sets
            var cardsOfRarityFromSet = selectedSet.CardSetContents.Where(x => x.Rarity.Contains(overallRarity) && !x.Name.Contains(exclusionString));
         
            if (cardsOfRarityFromSet.Count() < 1)
            {
                // TODO: log error
                return null;
            }

            var randomInt = _randomizer.Next(cardsOfRarityFromSet.Count());

            var fromSet = cardsOfRarityFromSet.ElementAtOrDefault(randomInt);

            var chosenCard = new Card
            {
                Name = fromSet.Name,
                Set = fromSet.Set,
                Rarity = fromSet.Rarity,
                Text = fromSet.Text,
                CardId = fromSet.CardId,
                Side = fromSet.Side,
                Type = fromSet.Type,
                SubType = fromSet.SubType
            };

            fromSet.Name += exclusionString;

            //selectedSet.CardSetContents = selectedSet.CardSetContents.Where(x => x.CardId != chosenCard?.CardId).AsList();
            
            return chosenCard;
            
        }
    }
}
