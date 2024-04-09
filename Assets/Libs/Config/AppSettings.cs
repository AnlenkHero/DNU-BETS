using UnityEngine;

namespace Libs.Config
{
    [CreateAssetMenu(fileName = "AppSettings", menuName = "Configuration/AppSettings")]
    public class AppSettings : ScriptableObject
    {
        public ApiSettings ApiSettings;
        public StorageSettings StorageSettings;
        public double BuffPrice;
        public double DefaultBalance;    
    }
}