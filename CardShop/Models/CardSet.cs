using CardShop.Constants;
using CardShop.Repositories.Models;

namespace CardShop.Models
{
    public class CardSet
    {
        public string CardSetName { get; set; }
        public List<Card> CardSetContents { get; set; } = new List<Card>();

        public CardSet() { }

        public CardSet(string cardSetName)
        {
            CardSetName = cardSetName;
        }

        public void FillSet(IEnumerable<Card> cardSet)
        {
            // clear contents
            CardSetContents = new List<Card>();

            foreach (var card in cardSet)
            {
                // how many instances of this card in the draw pile because of its rarity?
                var cardMultiplier = GetSetCardCountByRarity(card.Rarity);

                if (cardMultiplier < 0)
                {
                    // TODO: log error
                }
                for (int i = cardMultiplier; i > 0; i--)
                {
                    CardSetContents.Add(new Card
                    {
                        CardId = card.CardId,
                        Name = card.Name,
                        Text = card.Text,
                        Rarity = card.Rarity,
                        Set = card.Set,
                        Side = card.Side,
                        Type = card.Type,
                        SubType = card.SubType
                    });
                }
            }
        }
        private int GetSetCardCountByRarity(string rarity)
        {
            switch (CardSetName)
            {
                case CardSetConstants.Premiere:
                    switch (rarity)
                    {

                        case "C1": return 1;
                        case "C2": return 2;
                        case "C3": return 3;
                        case "U1": return 1;
                        case "U2": return 2;
                        case "R1": return 1;
                        case "R2": return 2;
                    }
                    return -1;
                case CardSetConstants.NewHope:
                    switch (rarity)
                    {

                        case "C1": return 1;
                        case "C2": return 2;
                        case "C3": return 3;
                        case "U1": return 1;
                        case "U2": return 2;
                        case "R1": return 1;
                        case "R2": return 2;
                    }
                    return -1;
                case CardSetConstants.Hoth:
                    switch (rarity)
                    {

                        case "C1": return 1;
                        case "C2": return 2;
                        case "C3": return 3;
                        case "U1": return 1;
                        case "U2": return 2;
                        case "R1": return 1;
                        case "R2": return 2;
                    }
                    return -1;
                case CardSetConstants.Dagobah:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                    }
                    return -1;
                case CardSetConstants.CloudCity:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                    }
                    return -1;
                case CardSetConstants.JabbasPalace:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                    }
                    return -1;
                case CardSetConstants.SpecialEdition:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Endor:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R": return 1;
                        case "CF": return 9;
                        case "UF": return 4;
                        case "RF": return 2;
                    }
                    return -1;
                case CardSetConstants.DeathStar2:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "UR": return 2;
                        case "R3": return 3;
                        case "R4": return 4;
                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Tatooine:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R1": return 1;
                        case "R3": return 3;
                        case "R4": return 4;
                    }
                    return -1;
                case CardSetConstants.Coruscant:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R1": return 1;
                        case "R3": return 3;
                        case "R4": return 4;
                    }
                    return -1;
                case CardSetConstants.Theed:
                    switch (rarity)
                    {

                        case "C": return 1;
                        case "U": return 1;
                        case "R1": return 1;
                        case "R3": return 3;
                        case "R4": return 4;
                    }
                    return -1;
                case CardSetConstants.Reflections1:
                    switch (rarity)
                    {

                        case "CF": return 3;
                        case "UF": return 2;
                        case "RF": return 1;
                    }
                    return -1;
                case CardSetConstants.Reflections2:
                    switch (rarity)
                    {

                        case "CF": return 3;
                        case "UF": return 2;
                        case "RF": return 1;
                        case "F1": return 1;
                        case "F2": return 1;
                    }
                    return -1;
                case CardSetConstants.Reflections3:
                    switch (rarity)
                    {

                        case "CF": return 3;
                        case "UF": return 2;
                        case "RF": return 1;
                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.PremiereEnhanced:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.CloudCityEnhanced:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.JabbasPalaceEnhanced:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.JediPack:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.ThirdAnthology:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.PremiereSealedDeck:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.JabbasPalaceSealedDeck:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Promotional:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Starter1:
                    switch (rarity)
                    {

                        case "F": return 1;
                    }
                    return -1;
                case CardSetConstants.Starter2:
                    switch (rarity)
                    {
                        case "F": return 1;
                    }
                    return -1;
                default:
                    return -1;
            }
        }
    }
}
