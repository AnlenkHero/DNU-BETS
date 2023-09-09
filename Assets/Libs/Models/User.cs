using System.Collections.Generic;
namespace Libs.Models 
{
    [System.Serializable]
    public class User
    {
        public string Token;
        public string UserName;
        public decimal Balance;
        public List<Bet> Bets;
    }
}