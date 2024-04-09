using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Libs.Models.RequestModels;


namespace Libs.Models
{
    [Serializable]
    public class Match
    {
        public int Id;
        public string ImageUrl;
        public string MatchTitle;
        public List<Contestant> Contestants;
        public string FinishedDateUtc;
        public bool IsBettingAvailable;
        public bool IsMatchCanceled;

        public Match() { }

        public Match(int id, MatchRequest matchRequest)
        {
            if (matchRequest == null)
            {
                throw new ArgumentException(nameof(matchRequest));
            }
            
            this.Id = id;
            this.ImageUrl = matchRequest.ImageUrl;
            this.MatchTitle = matchRequest.MatchTitle;
            this.Contestants = matchRequest.Contestants;
            this.FinishedDateUtc = matchRequest.FinishedDateUtc.ToString();
            this.IsBettingAvailable = matchRequest.IsBettingAvailable;
            this.IsMatchCanceled = matchRequest.IsMatchCanceled;
        }
    }
}