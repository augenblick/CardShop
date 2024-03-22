namespace CardShop.Models.Response
{
    public class GetAllAvailableCardSetInfoResponse
    {
        public int InfoCount { get; set; }
        public List<CardSetInfo> CardSetsInfo { get; set; } = new List<CardSetInfo>();
    }
}
