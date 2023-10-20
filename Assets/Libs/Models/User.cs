using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Libs.Models 
{
    [System.Serializable]
    public class User
    {
        public string id;
        public string userId;
        public string userName;
        public double balance;
        public string imageUrl;
        public List<BuffPurchase> buffPurchase = new List<BuffPurchase>();
    }
}