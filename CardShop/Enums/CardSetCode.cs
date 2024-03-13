using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardShop.Enums
{
    public enum CardSetCode
    {
        undefined = 0,
        Premiere,
        ANewHope,
        Hoth,
        Dagobah,
        CloudCity,
        JabbasPalace,
        SpecialEdition,
        Endor,
        DeathStar2,
        Tatooine,
        Coruscant,
        TheedPalace,
        PremiereTwoPlayer,
        JediPack,
        EsbTwoPlayer,
        RebelLeaders,
        TournamentSealedDeck,
        EnhancedPremiere,
        EnhancedCloudCity,
        EnhancedJabbasPalace,
        JabbasSealedDeck,
        Reflections2,
        ThirdAnthology,
        Reflections3
    }

    public static class CardSetHelpers
    {
        public static string GetCardSetCodeString(this CardSetCode cardSetCode)
        {
            switch (cardSetCode)
            {
                case CardSetCode.Premiere:
                    return "pr";

                case CardSetCode.ANewHope:
                    return "anh";

                case CardSetCode.Hoth:
                    return "hoth";

                case CardSetCode.Dagobah:
                    return "dah";

                case CardSetCode.CloudCity:
                    return "cc";

                case CardSetCode.JabbasPalace:
                    return "jp";

                case CardSetCode.SpecialEdition:
                    return "se";

                case CardSetCode.Endor:
                    return "edr";

                case CardSetCode.DeathStar2:
                    return "ds2";

                case CardSetCode.Tatooine:
                    return "tat";

                case CardSetCode.Coruscant:
                    return "cor";

                case CardSetCode.TheedPalace:
                    return "tp";

                case CardSetCode.PremiereTwoPlayer:
                    return "2pp";

                case CardSetCode.JediPack:
                    return "jpack";

                case CardSetCode.EsbTwoPlayer:
                    return "2pesb";

                case CardSetCode.RebelLeaders:
                    return "rlp";

                case CardSetCode.TournamentSealedDeck:
                    return "otsd";

                case CardSetCode.EnhancedPremiere:
                    return "epp";

                case CardSetCode.EnhancedCloudCity:
                    return "ecc";

                case CardSetCode.EnhancedJabbasPalace:
                    return "ejp";

                case CardSetCode.JabbasSealedDeck:
                    return "jpsd";

                case CardSetCode.Reflections2:
                    return "ref2";

                case CardSetCode.ThirdAnthology:
                    return "ta";

                case CardSetCode.Reflections3:
                    return "ref3";

                default:
                    return "unknown";

            }
        }

        public static CardSetCode GetCardSetCode(string cardSetCodeString)
        {
            switch (cardSetCodeString)
            {
                case "pr":
                    return CardSetCode.Premiere;

                case "anh": 
                    return CardSetCode.ANewHope;

                case "hoth" :
                    return CardSetCode.Hoth;

                case "dah":
                    return CardSetCode.Dagobah;

                case "cc":
                    return CardSetCode.CloudCity;

                case "jp":
                    return CardSetCode.JabbasPalace;

                case "se":
                    return CardSetCode.SpecialEdition;

                case "edr":
                    return CardSetCode.Endor;

                case "ds2":
                    return CardSetCode.DeathStar2;

                case "tat":
                    return CardSetCode.Tatooine;

                case "cor":
                    return CardSetCode.Coruscant;

                case "tp":
                    return CardSetCode.TheedPalace;

                case "2pp":
                    return CardSetCode.PremiereTwoPlayer;

                case "jpack":
                    return CardSetCode.JediPack;

                case "2pesb":
                    return CardSetCode.EsbTwoPlayer;

                case "rlp":
                    return CardSetCode.RebelLeaders;

                case "otsd":
                    return CardSetCode.TournamentSealedDeck;

                case "epp":
                    return CardSetCode.EnhancedPremiere;

                case "ecc":
                    return CardSetCode.EnhancedCloudCity;

                case "ejp":
                    return CardSetCode.EnhancedJabbasPalace;

                case "jpsd":
                    return CardSetCode.JabbasSealedDeck;

                case "ref2":
                    return CardSetCode.Reflections2;

                case "ta":
                    return CardSetCode.ThirdAnthology;

                case "ref3":
                    return CardSetCode.Reflections3;

                default:
                    return CardSetCode.undefined;

            }
        }
    }
    
}
