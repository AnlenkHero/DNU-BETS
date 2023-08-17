using Firebase;
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
        var app = FirebaseApp.Create();
       await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
        loader.storage = FirebaseStorage.DefaultInstance;
        return loader;
    }
    

    public async UniTask<List<Match>> FetchMatchesData()
    {
        UnityWebRequest www = UnityWebRequest.Get(matchesJsonUrl);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch matches data: " + www.error);
            return null;
        }
        else
        {
            string json = www.downloadHandler.text;
            return JsonConvert.DeserializeObject<List<Match>>(json);
        }
    }
}

