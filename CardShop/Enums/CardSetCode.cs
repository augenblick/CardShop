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
            switch(cardSetCode)
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
    }
}
