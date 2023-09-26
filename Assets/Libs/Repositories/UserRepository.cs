using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Models;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using UnityEngine;

namespace Libs.Repositories
{
    public static class UserRepository
    {
        private const string FirebaseDbUrl = "https://wwe-bets-default-rtdb.europe-west1.firebasedatabase.app/";

        public static Promise<User> GetUserByUserId(string userId)
        {
            return new Promise<User>((resolve, reject) =>
            {
                string queryUrl = $"{FirebaseDbUrl}users.json?orderBy=\"userId\"&equalTo=\"{userId}\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawUsers =
                        JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Text);
                    if (rawUsers == null || !rawUsers.Any())
                    {
                        reject(new Exception("User not found for provided UserID"));
                        return;
                    }

                    var firstUserKey = rawUsers.Keys.First();
                    var rawUser = rawUsers[firstUserKey];

                    User user = new User
                    {
                        userId = firstUserKey,
                        userName = rawUser["userName"] as string,
                        Balance = Convert.ToDouble(rawUser["Balance"])
                    };

                    resolve(user);
                }).Catch(error => { reject(new Exception($"Error retrieving user by UserID: {error.Message}")); });
            });
        }

        public static IPromise<string> SaveUser(User user)
        {
            var promise = new Promise<string>();

            if (string.IsNullOrEmpty(user.userId) || string.IsNullOrEmpty(user.userName))
            {
                promise.Reject(new Exception("UserID or UserName is null or empty"));
                return promise;
            }

            RestClient.Post($"{FirebaseDbUrl}users.json", user).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("name", out string newUserId))
                {
                    promise.Resolve(newUserId);
                }
                else
                {
                    promise.Reject(new Exception("User ID not returned from Firebase"));
                }
            }).Catch(error => { promise.Reject(error); });


            return promise;
        }

        public static IPromise<bool> UpdateUserBalance(string userId, double amountToChange)
        {
            var promise = new Promise<bool>();
            //TODO get by data by firebase id
            GetUserByUserId(userId).Then(user =>
            {
                user.Balance = amountToChange;
                string keyUrlPart = $"{FirebaseDbUrl}users/{user.userId}.json";
                Debug.Log(keyUrlPart);
                user.userId = userId;
                RestClient.Put(keyUrlPart, user).Then(response => { promise.Resolve(true); }).Catch(error =>
                {
                    promise.Reject(new Exception($"Error updating user balance: {error.Message}"));
                });
            }).Catch(error =>
            {
                Debug.Log($"error {error.Message}");
                promise.Reject(new Exception($"Error retrieving user by UserID for balance update: {error.Message}"));
            });

            return promise;
        }
        
        public static Promise<double> GetUserBalanceById(string userId)
        {
            return new Promise<double>((resolve, reject) =>
            {
                string queryUrl = $"{FirebaseDbUrl}users.json?orderBy=\"userId\"&equalTo=\"{userId}\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawUsers =
                        JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(response.Text);
                    if (rawUsers == null || !rawUsers.Any())
                    {
                        reject(new Exception("User not found for provided UserID"));
                        return;
                    }

                    var firstUserKey = rawUsers.Keys.First();
                    var rawUser = rawUsers[firstUserKey];

                    double balance = Convert.ToDouble(rawUser["Balance"]);
                    resolve(balance);
                }).Catch(error => { reject(new Exception($"Error retrieving user balance by UserID: {error.Message}")); });
            });
        }
    }
}