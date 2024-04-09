namespace Libs.Models 
{
    [System.Serializable]
    public class Bet
    {
        public int BetId;
        public int MatchId;
        public int ContestantId;
        public double BetAmount;
        public int UserId;
        public bool IsActive;
    }
}