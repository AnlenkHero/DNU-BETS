using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using UnityEngine;
using Libs.Models;

namespace Libs.Repositories
{
    public static class BetsRepository
    {
        private const string FirebaseDbUrl = "https://wwe-bets-default-rtdb.europe-west1.firebasedatabase.app/";

        public static IPromise<string> SaveBet(Bet bet)
        {
            var promise = new Promise<string>();

            string validationMessage = ValidateBet(bet);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            RestClient.Post($"{FirebaseDbUrl}bets.json", bet).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("name", out string newBetId))
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

        public static IPromise<ResponseHelper> UpdateBet(string betId, Bet betToUpdate)
        {
            string url = $"{FirebaseDbUrl}bets/{betId}.json";
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

            string url = $"{FirebaseDbUrl}bets/{betId}.json";
            return RestClient.Delete(url);
        }

        public static Promise<Bet> GetBetById(string betId)
        {
            return new Promise<Bet>((resolve, reject) =>
            {
                RestClient.Get($"{FirebaseDbUrl}bets/{betId}.json").Then(response =>
                {
                    var rawBet = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Text);

                    if (rawBet == null || !rawBet.Any())
                    {
                        reject(new Exception("Bet not found"));
                        return;
                    }

                    Bet bet = new Bet
                    {
                        MatchId = rawBet["MatchId"] as string,
                        ContestantId = rawBet["ContestantId"] as string,
                        BetAmount = Convert.ToDouble(rawBet["BetAmount"])
                    };

                    resolve(bet);
                }).Catch(error => { reject(new Exception($"Error retrieving bet by ID: {error.Message}")); });
            });
        }
        
        public static Promise<List<Bet>> GetAllBetsByUserId(string userId)
        {
            return new Promise<List<Bet>>((resolve, reject) =>
            {
                string queryUrl = $"{FirebaseDbUrl}bets.json?orderBy=\"UserId\"&equalTo=\"{userId}\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawBets = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Text);

                    if (rawBets == null || !rawBets.Any())
                    {
                        reject(new Exception("No bets found for the user"));
                        return;
                    }

                    List<Bet> bets = new List<Bet>();
                    foreach (var rawBetKey in rawBets.Keys)
                    {
                        var rawBet = rawBets[rawBetKey];

                        Bet bet = new Bet
                        {
                            MatchId = rawBet["MatchId"] as string,
                            ContestantId = rawBet["ContestantId"] as string,
                            BetAmount = Convert.ToDouble(rawBet["BetAmount"]),
                            UserId = rawBet["UserId"] as string
                        };

                        bets.Add(bet);
                    }
                    
                    resolve(bets);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving bets by user ID: {error.Message}"));
                });
            });
        }

        private static string ValidateBet(Bet bet)
        {
            if (string.IsNullOrEmpty(bet.MatchId))
                return "Match ID cannot be empty.";
            if (string.IsNullOrEmpty(bet.ContestantId))
                return "Contestant ID cannot be empty.";
            if (bet.BetAmount <= 0)
                return "Bet amount should be greater than 0.";
            if (string.IsNullOrEmpty(bet.UserId))
                return "User ID cannot be empty.";

            return null;
        }
    }
}
