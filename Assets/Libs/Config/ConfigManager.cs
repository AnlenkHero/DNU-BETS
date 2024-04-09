using UnityEngine;
namespace Libs.Config
{
    public class ConfigManager : MonoBehaviour
    {
        public static AppSettings Settings { get; private set; }

        [SerializeField] 
        private AppSettings settings; 

        void Awake()
        {
            if (Settings == null)
            {
                Settings = settings;
                DontDestroyOnLoad(gameObject);
                return;
            }
            
            Destroy(gameObject); 
        }
    }
}
