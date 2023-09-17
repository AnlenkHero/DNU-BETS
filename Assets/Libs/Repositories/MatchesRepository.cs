using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IPromise<string> Save(MatchRequest match, Texture2D imageTexture)
        {
            var promise = new Promise<string>();

            string validationMessage = ValidateMatch(match);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            UploadImage(imageTexture, $"{Guid.NewGuid()}.png").Then(imageUrl =>
            {
                match.ImageUrl = imageUrl;

                RestClient.Post($"{FirebaseDbUrl}matches.json", match).Then(response =>
                {
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

                    if (jsonResponse != null && jsonResponse.TryGetValue("name", out string newMatchId))
                    {
                        promise.Resolve(newMatchId);
                    }
                    else
                    {
                        promise.Reject(new Exception("Match id is not returned"));
                    }
                }).Catch(error => { promise.Reject(error); });
            }).Catch(error => { promise.Reject(error); });

            return promise;
        }

        public static IPromise<ResponseHelper> DeleteMatch(string matchId)
        {
            if (matchId == null)
            {
                var promise = new Promise<ResponseHelper>();
                promise.Reject(new Exception("Id is null"));
                return promise;
            }

            string url = $"{FirebaseDbUrl}matches/{matchId}.json";
            return RestClient.Delete(url);
        }

        public static IPromise<ResponseHelper> UpdateMatch(string matchId, MatchRequest matchToUpdate,
            Texture2D imageToChange = null, string imageURL = null)
        {
            string url = $"{FirebaseDbUrl}matches/{matchId}.json";
            var promise = new Promise<ResponseHelper>();
            string validationMessage = ValidateMatch(matchToUpdate);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            if (imageToChange != null)
            {
                DeleteImage(imageURL);
                UploadImage(imageToChange, $"{Guid.NewGuid()}.png").Then(image =>
                {
                    matchToUpdate.ImageUrl = image;
                    RestClient.Put(url, matchToUpdate).Then(x => promise.Resolve(x))
                        .Catch(error => promise.Reject(error));
                });
                return promise;
            }

            if (String.IsNullOrWhiteSpace(imageURL))
            {
                promise.Reject(new Exception("image url not provided"));
                return promise;
            }

            if (imageToChange == null && String.IsNullOrWhiteSpace(imageURL) != true)
            {
                matchToUpdate.ImageUrl = imageURL;
            }

            return RestClient.Put(url, matchToUpdate);
        }

        public static Promise<Match> GetMatchById(string matchId)
        {
            return new Promise<Match>((resolve, reject) =>
            {
                RestClient.Get($"{FirebaseDbUrl}matches/{matchId}.json").Then(response =>
                {
                    var rawMatch = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Text);

                    if (rawMatch == null || !rawMatch.Any())
                    {
                        reject(new Exception("Match not found"));
                        return;
                    }

                    Match match = new Match
                    {
                        Id = matchId,
                        ImageUrl = rawMatch["ImageUrl"] as string,
                        MatchTitle = rawMatch["MatchTitle"] as string,
                        IsBettingAvailable = (bool)rawMatch["IsBettingAvailable"],
                        FinishedDateUtc = rawMatch.TryGetValue("FinishedDateUtc", out var finishedDate)
                            ? finishedDate as string
                            : null,
                        Contestants = new List<Contestant>()
                    };

                    if (rawMatch.TryGetValue("Contestants", out var value))
                    {
                        string contestantsString = Convert.ToString(value);
                        List<Dictionary<string, object>> rawContestants =
                            JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(contestantsString);
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

                    resolve(match);
                }).Catch(error => { reject(new Exception($"Error retrieving match by ID: {error.Message}")); });
            });
        }

        public static Promise<List<Match>> GetNotFinishedMatches()
        {
            return new Promise<List<Match>>((resolve, reject) =>
            {
                string queryUrl = $"{FirebaseDbUrl}matches.json?orderBy=\"FinishedDateUtc\"&equalTo=\"\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Text);
                    List<Match> matches = ExtractMatchesFromRawData(rawMatches);
                    resolve(matches);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving not finished matches: {error.Message}"));
                });
            });
        }
        public static Promise<List<Match>> GetBettingAvailableMatches()
        {
            return new Promise<List<Match>>((resolve, reject) =>
            {
                string queryUrl = $"{FirebaseDbUrl}matches.json?orderBy=\"IsBettingAvailable\"&equalTo=true";
                Debug.Log(queryUrl);

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Text);
                    List<Match> matches = ExtractMatchesFromRawData(rawMatches);
                    resolve(matches);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving betting available matches: {error.Message}"));
                });
            });
        }
        private static List<Match> ExtractMatchesFromRawData(Dictionary<string, Dictionary<string, object>> rawMatches)
        {
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

                if (rawMatch.TryGetValue("Contestants", out var contestantsValue))
                {
                    string contestantsString = Convert.ToString(contestantsValue);
                    List<Dictionary<string, object>> rawContestants =
                        JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(contestantsString);
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
            return matches;
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

                RestClient.Request(requestData).Then(_ =>
                {
                    GetDownloadURL(fileName).Then(resolve).Catch(error =>
                    {
                        reject(new Exception($"Error retrieving download URL: {error.Message}"));
                    });
                }).Catch(error => { reject(new Exception($"Error uploading image: {error.Message}")); });
            });
        }

        public static IPromise<ResponseHelper> DeleteImage(string imageUrl)
        {
            return new Promise<ResponseHelper>((resolve, reject) =>
            {
                var uri = new Uri(imageUrl);
                string fileName = System.Web.HttpUtility.UrlDecode(uri.Segments.Last());

                string deleteEndpoint = $"{FirebaseStorageURL}/o/{fileName}";

                RestClient.Request(new RequestHelper
                    {
                        Uri = deleteEndpoint,
                        Method = "DELETE",
                        Headers = new Dictionary<string, string>
                        {
                            { "Content-Type", "image/png" }
                        },
                    })
                    .Then(resolve)
                    .Catch(error => { reject(new Exception($"Failed to delete image: {error.Message}")); });
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