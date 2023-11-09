using System;
using System.Collections.Generic;

namespace Libs.Models.RequestModels
{
    [Serializable]
    public class MatchRequest
    {
        public string ImageUrl;
        public string MatchTitle;
        public List<ContestantRequest> Contestants;
        public string FinishedDateUtc;
        public bool IsBettingAvailable;
        public bool IsMatchCanceled;
    }
}