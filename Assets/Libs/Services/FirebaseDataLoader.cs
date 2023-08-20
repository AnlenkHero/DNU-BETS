using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Storage;
using Libs.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Libs.Services
{
    public class FirebaseDataLoader 
    {
        private FirebaseStorage _storage;
        private const string MatchesJsonUrl = "https://firebasestorage.googleapis.com/v0/b/wwe-bets.appspot.com/o/matches.json?alt=media&token=0aefbc74-5a53-409d-9d3e-bfaad8da10e1"; // Replace with the Download URL from Firebase Storage

        public FirebaseDataLoader(FirebaseStorage storage)
        {
            _storage = storage;
        }
    
        public static async UniTask<List<Match>> FetchMatchesData()
        {
            UnityWebRequest www = UnityWebRequest.Get(MatchesJsonUrl);
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch matches data: " + www.error);
                return null;
            }
        
            string json = www.downloadHandler.text;
            return JsonConvert.DeserializeObject<List<Match>>(json);
        }
    }
}

