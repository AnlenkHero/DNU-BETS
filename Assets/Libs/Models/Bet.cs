namespace Libs.Models 
{
    [System.Serializable]
    public class Bet
    {
        public string Id;
        public string MatchId { get; set; }
        public string ContestantId { get; set; }
        public decimal BetAmount { get; set; }
    }
}