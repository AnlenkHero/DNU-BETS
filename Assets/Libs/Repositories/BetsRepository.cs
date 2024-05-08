using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Config;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using Libs.Models;
using Libs.Models.RequestModels;

namespace Libs.Repositories
{
    public static class BetsRepository
    {
        private static readonly ApiSettings APISettings = ConfigManager.Settings.ApiSettings;
        private static readonly string BaseUrl = $"{ConfigManager.Settings.ApiSettings.Url}/api/bet";
        
        public static IPromise<int> SaveBet(BetRequest betRequest)
        {
            var promise = new Promise<int>();

            string validationMessage = ValidateBet(betRequest);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            string url = $"{APISettings.Url}/api/bet";
            
            RestClient.Post(url, betRequest).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, int>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("id", out int newBetId))
                {
                    promise.Resolve(newBetId);
                }
                else
                {
                    promise.Reject(new Exception("Bet id is not returned"));
                }
            }).Catch(error => { promise.Reject(error); });

            return promise;
        }

        public static IPromise<ResponseHelper> UpdateBet(string betId, BetRequest betToUpdate)
        {
            string url = $"{APISettings.Url}/api/bet/{betId}";
            
            var promise = new Promise<ResponseHelper>();
            
            string validationMessage = ValidateBet(betToUpdate);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            return RestClient.Put(url, betToUpdate);
        }

        public static IPromise<ResponseHelper> DeleteBet(string betId)
        {
            if (betId == null)
            {
                var promise = new Promise<ResponseHelper>();
                promise.Reject(new Exception("Id is null"));
                return promise;
            }

            string url = $"{APISettings.Url}/api/bet/{betId}";
            return RestClient.Delete(url);
        }

        public static Promise<Bet> GetBetById(string betId)
        {
            return new Promise<Bet>((resolve, reject) =>
            {
                RestClient.Get($"{APISettings}/api/bet/{betId}").Then(response =>
                {
                    Bet bet = JsonConvert.DeserializeObject<Bet>(response.Text);

                    if (bet == null)
                    {
                        reject(new Exception("Bet not found"));
                        return;
                    }

                    resolve(bet);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving bet by ID: {error.Message}"));
                });
            });
        }

        public static Promise<List<Bet>> GetAllBets(int? userId = null, int? matchId = null) //TODO pagination
        {
            if (userId == null && matchId == null)
            {
                Promise<List<Bet>> promise = new Promise<List<Bet>>();
                promise.Reject(new ArgumentNullException("Cannot get all bets"));
            }
            
            return new Promise<List<Bet>>((resolve, reject) =>
            {
                string url = $"{APISettings.Url}/api/bet?userId={userId}&matchId={matchId}";

                RestClient.Get(url).Then(response =>
                {
                    var bets = JsonConvert.DeserializeObject<List<Bet>>(response.Text);

                    resolve(bets);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving bets by match ID: {error.Message}"));
                });
            });
        }
        
        private static string ValidateBet(BetRequest bet)
        {
            if (bet.MatchId <= 0)
                return "Match ID cannot be empty.";
            if (bet.ContestantId < 0)
                return "Contestant ID cannot be empty.";
            if (bet.BetAmount <= 0)
                return "Bet amount should be greater than 0.";
            if (bet.UserId <= 0)
                return "User ID should be greater than 0.";

            return null;
        }
    }
}
