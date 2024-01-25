using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Helpers;
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

            ImageHelper.UploadImage(imageTexture, $"{Guid.NewGuid()}.png").Then(imageUrl =>
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
                ImageHelper.DeleteImage(imageURL);
                ImageHelper.UploadImage(imageToChange, $"{Guid.NewGuid()}.png").Then(image =>
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
                RestClient.Get($"{FirebaseDbUrl}matches.json").Then(response =>
                {
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, MatchRequest>>(response.Text);

                    if (rawMatches == null || !rawMatches.Any())
                    {
                        reject(new Exception("No matches found"));
                        return;
                    }

                    var matches = ExtractMatchesFromRawData(rawMatches);
                    var match = matches.FirstOrDefault(m => m.Id == matchId);

                    if (match == null)
                    {
                        reject(new Exception("Match not found"));
                        return;
                    }

                    resolve(match);
                }).Catch(error => { reject(new Exception($"Error retrieving matches: {error.Message}")); });
            });
        }


        public static Promise<List<Match>> GetNotFinishedMatches()
        {
            return new Promise<List<Match>>((resolve, reject) =>
            {
                string queryUrl = $"{FirebaseDbUrl}matches.json?orderBy=\"FinishedDateUtc\"&equalTo=\"\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, MatchRequest>>(response.Text);
                    List<Match> matches = ExtractMatchesFromRawData(rawMatches);
                    resolve(matches);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving not finished matches: {error.Message}"));
                });
            });
        }
        
        public static Promise<List<Match>> GetAllMatches()
        {
            return new Promise<List<Match>>((resolve, reject) =>
            {
                string url = $"{FirebaseDbUrl}matches.json";

                RestClient.Get(url).Then(response =>
                {
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, MatchRequest>>(response.Text);
                    if (rawMatches == null || !rawMatches.Any())
                    {
                        reject(new Exception("No matches found"));
                        return;
                    }

                    List<Match> matches = ExtractMatchesFromRawData(rawMatches);
                    resolve(matches);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving all matches: {error.Message}"));
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
                    var rawMatches = JsonConvert.DeserializeObject<Dictionary<string, MatchRequest>>(response.Text);
                    List<Match> matches = ExtractMatchesFromRawData(rawMatches);
                    resolve(matches);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving betting available matches: {error.Message}"));
                });
            });
        }
        private static List<Match> ExtractMatchesFromRawData(Dictionary<string, MatchRequest> rawMatches)
        {
            List<Match> matches = new List<Match>();
            foreach (var rawMatchKey in rawMatches.Keys)
            {
                var rawMatch = rawMatches[rawMatchKey];

                Match match = new Match
                {
                    Id = rawMatchKey,
                    ImageUrl = rawMatch.ImageUrl,
                    MatchTitle = rawMatch.MatchTitle,
                    IsBettingAvailable = rawMatch.IsBettingAvailable,
                    IsMatchCanceled = rawMatch.IsMatchCanceled,
                    FinishedDateUtc = rawMatch.FinishedDateUtc,
                    Contestants = new List<Contestant>()
                };
                
                for (int i = 0; i < rawMatch.Contestants.Count; i++)
                {
                    var contestantRequest = rawMatch.Contestants[i];

                    match.Contestants.Add(new Contestant
                    {
                        Id = i.ToString(),
                        Name = contestantRequest.Name,         
                        Coefficient = contestantRequest.Coefficient,  
                        Winner = contestantRequest.Winner      
                    });
                }
                
                matches.Add(match);
            }
            return matches;
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