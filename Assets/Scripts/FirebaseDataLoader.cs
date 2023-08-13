using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

public class FirebaseDataLoader 
{
    private FirebaseStorage storage;
    private const string matchesJsonUrl = "https://firebasestorage.googleapis.com/v0/b/wwe-bets.appspot.com/o/matches.json?alt=media&token=0aefbc74-5a53-409d-9d3e-bfaad8da10e1"; // Replace with the Download URL from Firebase Storage

    public static async UniTask<FirebaseDataLoader> Create()
    {
        var loader = new FirebaseDataLoader();
        await FirebaseApp.CheckAndFixDependenciesAsync();
        FirebaseApp.Create();
        loader.storage = FirebaseStorage.DefaultInstance;
        return loader;
    }

    private FirebaseDataLoader()
    {
        
    }

    public async UniTask<List<Match>> FetchMatchesData()
    {
        UnityWebRequest www = UnityWebRequest.Get(matchesJsonUrl);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch matches data: " + www.error);
            return null; // or handle the error as you see fit
        }
        else
        {
            string json = www.downloadHandler.text;
            return JsonConvert.DeserializeObject<List<Match>>(json);
        }
    }
}

