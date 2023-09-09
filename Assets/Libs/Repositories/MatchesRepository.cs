using System;
using System.Collections.Generic;
using Libs.Models;
using Libs.Models.RequestModels;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using UnityEngine;

namespace Libs.Repositories
{
    public static class MatchesRepository
    {
        private const string FirebaseDbUrl = "https://wwe-bets-default-rtdb.europe-west1.firebasedatabase.app/";
        private const string FirebaseStorageURL = "https://firebasestorage.googleapis.com/v0/b/wwe-bets.appspot.com";

        public static IPromise<ResponseHelper> Save(MatchRequest match, Texture2D imageTexture)
        {
            var promise = new Promise<ResponseHelper>();

            string validationMessage = ValidateMatch(match);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            UploadImage(imageTexture, $"{Guid.NewGuid()}.png").Then(imageUrl =>
            {
                match.ImageUrl = imageUrl;

                RestClient.Post($"{FirebaseDbUrl}matches.json", match).Then(response => { promise.Resolve(response); })
                    .Catch(error => { promise.Reject(error); });
            }).Catch(error => { promise.Reject(error); });

            return promise;
        }

        public static Promise<List<Match>> GetAllMatches()
        {
            return new Promise<List<Match>>((resolve, reject) =>
            {
                RestClient.Get($"{FirebaseDbUrl}matches.json").Then(response =>
                {
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,object>>>(response.Text);

                    List<Match> matches = new List<Match>();
                    foreach (var rawMatchKey in rawMatches.Keys)
                    {
                        var rawMatch = rawMatches[rawMatchKey];

                        Match match = new Match
                        {
                            Id = rawMatchKey,
                            ImageUrl = rawMatch["ImageUrl"] as string,
                            MatchTitle = rawMatch["MatchTitle"] as string,
                            IsBettingAvailable = (bool)rawMatch["IsBettingAvailable"],
                            FinishedDateUtc = rawMatch.TryGetValue("FinishedDateUtc", out var finishedDate)
                                ? finishedDate as string
                                : null,
                            Contestants = new List<Contestant>()
                        };

                        if (rawMatch.ContainsKey("Contestants"))
                        {
                            string contestantsString = Convert.ToString(rawMatch["Contestants"]);
                            List<Dictionary<string, object>> rawContestants = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(contestantsString);
                            for (int i = 0; i < rawContestants.Count; i++)
                            {
                                var contestantDict = rawContestants[i];
                                match.Contestants.Add(new Contestant
                                {
                                    Id = i.ToString(),
                                    Name = contestantDict["Name"] as string,
                                    Coefficient = Convert.ToDouble(contestantDict["Coefficient"]),
                                    Winner = (bool)contestantDict["Winner"]
                                });
                            }
                        }

                        matches.Add(match);
                    }

                    resolve(matches);
                }).Catch(error => { reject(new Exception($"Error retrieving matches: {error.Message}")); });
            });
        }

        private static Promise<string> UploadImage(Texture2D imageToUpload, string fileName)
        {
            return new Promise<string>((resolve, reject) =>
            {
                byte[] imageBytes = imageToUpload.EncodeToPNG();

                var headers = new Dictionary<string, string>
                {
                    { "Content-Type", "image/png" }
                };

                var requestData = new RequestHelper
                {
                    Uri = $"{FirebaseStorageURL}/o?uploadType=media&name={fileName}",
                    Method = "POST",
                    BodyRaw = imageBytes,
                    Headers = headers
                };

                RestClient.Request(requestData).Then(response =>
                {
                    GetDownloadURL(fileName).Then(resolve).Catch(error =>
                    {
                        reject(new Exception($"Error retrieving download URL: {error.Message}"));
                    });
                }).Catch(error => { reject(new Exception($"Error uploading image: {error.Message}")); });
            });
        }

        private static Promise<string> GetDownloadURL(string fileName)
        {
            return new Promise<string>((resolve, reject) =>
            {
                RestClient.Get($"{FirebaseStorageURL}/o/{fileName}").Then(response =>
                {
                    string downloadToken = JsonUtility.FromJson<DownloadUrlResponse>(response.Text).downloadTokens;
                    string completeUrl = $"{FirebaseStorageURL}/o/{fileName}?alt=media&token={downloadToken}";
                    resolve(completeUrl);
                }).Catch(error => { reject(new Exception($"Error retrieving download URL: {error.Message}")); });
            });
        }

        [Serializable]
        public class DownloadUrlResponse
        {
            public string downloadTokens;
        }

        private static string ValidateMatch(MatchRequest match)
        {
            if (string.IsNullOrEmpty(match.MatchTitle))
                return "Match title cannot be empty.";
            if (match.Contestants == null || match.Contestants.Count < 2)
                return "There should be at least two contestants.";

            foreach (var contestant in match.Contestants)
            {
                if (string.IsNullOrEmpty(contestant.Name))
                    return "Contestant name cannot be empty.";
                if (contestant.Coefficient <= 0)
                    return "Contestant coefficient should be greater than 0.";
            }

            return null;
        }
    }
}