using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Libs.Models 
{
    [System.Serializable]
    public class User
    {
        public int id;
        public string token;
        public string userName;
        public double balance;
        public string imageUrl;
        public List<BuffPurchase> buffPurchase = new List<BuffPurchase>();
    }
}