using System.Collections.Generic;
namespace Libs.Models 
{
    [System.Serializable]
    public class User
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public decimal Balance { get; set; }
        public List<Bet> Bets { get; set; }
    }
}