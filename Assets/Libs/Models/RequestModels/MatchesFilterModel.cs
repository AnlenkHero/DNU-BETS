using System;

namespace Libs.Models.RequestModels
{
    [Serializable]
    public class MatchesFilterModel
    {
        public bool? Available;
        public bool? Finished;
        public bool WithBets;
    }
}