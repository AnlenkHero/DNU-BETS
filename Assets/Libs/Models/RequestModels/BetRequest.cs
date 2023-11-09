namespace Libs.Models.RequestModels
{
    [System.Serializable]
    public class BetRequest
    {
        public string MatchId;
        public string ContestantId;
        public double BetAmount;
        public string UserId;
        public bool IsActive;
    }
}