using System.Collections.Generic;

namespace Libs.Models
{
    [System.Serializable]
    public class Match
    {
        public string Id;
        public string ImageUrl;
        public string MatchTitle;
        public List<Contestant> Contestants;
        public bool IsFinished;
        public bool IsStarted;
    }
}