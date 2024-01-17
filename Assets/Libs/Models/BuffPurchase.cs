using System;

namespace Libs.Models
{
    [Serializable]
    public class BuffPurchase
    {
        public double price;
        public bool isProcessed;
        public int quantity;
        public string date;
    }
}