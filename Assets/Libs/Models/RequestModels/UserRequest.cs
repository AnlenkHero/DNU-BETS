using System.Collections.Generic;

namespace Libs.Models.RequestModels 
{
    [System.Serializable]
    public class UserRequest
    {
        public string userId;
        public string userName;
        public double balance;
        public string imageUrl;
        public List<BuffPurchase> buffPurchase = new List<BuffPurchase>();
    }
}