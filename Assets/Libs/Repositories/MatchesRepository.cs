using System;
using System.Collections.Generic;
using Libs.Config;
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
        private static readonly ApiSettings APISettings = ConfigManager.Settings.ApiSettings;
        
        public static IPromise<int> Create(MatchRequest match, Texture2D imageTexture)
        {
            var promise = new Promise<int>();

            string validationMessage = ValidateMatch(match);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            Texture2D resizedImage = ImageProcessing.ResizeAndCompressTexture(imageTexture, 1000, 1000, 75);

            ImageHelper.UploadImage(resizedImage, $"{Guid.NewGuid()}.png").Then(imageUrl =>
            {
                match.ImageUrl = imageUrl;
                string url = $"{APISettings.Url}/api/match";
                
                RestClient.Post(url, match).Then(response =>
                {
                    Dictionary<string,int> jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, int>>(response.Text);

                    if (jsonResponse != null && jsonResponse.TryGetValue("id", out int newMatchId))
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

        public static IPromise<ResponseHelper> DeleteMatch(int matchId)
        {
            if (matchId <= 0)
            {
                var promise = new Promise<ResponseHelper>();
                promise.Reject(new ArgumentOutOfRangeException("Id cannot be less or equal to 0"));
                return promise;
            }

            string url = $"{APISettings.Url}/api/match/{matchId}";
            return RestClient.Delete(url);
        }

        public static IPromise<ResponseHelper> UpdateMatch(int matchId, MatchRequest matchToUpdate, Texture2D imageToChange = null)
        {
            if (matchId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matchId));
            }

            if (matchToUpdate == null)
            {
                throw new ArgumentNullException(nameof(matchToUpdate));
            }

            string url = $"{APISettings.Url}/api/match/{matchId}";
            var promise = new Promise<ResponseHelper>();
            string validationMessage = ValidateMatch(matchToUpdate);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            if (imageToChange != null)
            {
                ImageHelper.DeleteImage(matchToUpdate.ImageUrl);
                Texture2D resizedImage = ImageProcessing.ResizeAndCompressTexture(imageToChange, 400, 400, 50);
                
                ImageHelper.UploadImage(resizedImage, $"{Guid.NewGuid()}.png").Then(image =>
                {
                    matchToUpdate.ImageUrl = image;
                    RestClient.Put(url, matchToUpdate).Then(x => promise.Resolve(x))
                        .Catch(error => promise.Reject(error));
                });
                
                return promise;
            }

            return RestClient.Put(url, matchToUpdate);
        }

        public static Promise<Match> GetMatchById(int matchId)
        {
            string url = $"{APISettings.Url}/api/match/{matchId}";
            
            return new Promise<Match>((resolve, reject) =>
            {
                RestClient.Get(url).Then(response =>
                {
                    resolve(JsonConvert.DeserializeObject<Match>(response.Text));
                }).Catch(error => { reject(new Exception($"Error retrieving match by id: {error.Message}")); });
            });
        }


        public static IPromise<List<Match>> GetAllMatches(bool? available = null, bool? finished = null)
        {
            string queryUrl = $"{APISettings.Url}/api/match?available={available}&finished={finished}";

            return new Promise<List<Match>>((resolve, reject) =>
            {
                RestClient.Get(queryUrl)
                    .Then(response => { resolve(JsonConvert.DeserializeObject<List<Match>>(response.Text)); })
                    .Catch(e =>
                    {
                        var exception = e as RequestException;
                        
                        Debug.LogError(exception.Message); 
                        reject(exception);
                    });
            });
        }

        public static IPromise<List<Match>> GetBettingAvailableMatches() //TODO replace
        {
            string queryUrl = $"{APISettings.Url}/match?available=true";

            return RestClient.Get<List<Match>>(queryUrl);
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