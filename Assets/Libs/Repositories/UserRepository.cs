using System;
using System.Collections.Generic;
using Libs.Config;
using Libs.Helpers;
using Libs.Models;
using Libs.Models.RequestModels;
using Newtonsoft.Json;
using Proyecto26;
using RSG;

namespace Libs.Repositories
{
    public static class UserRepository
    {
        private static readonly string BaseUrl = $"{ConfigManager.Settings.ApiSettings.Url}/api/user";
         
        public static Promise<User> GetUserByToken(string token)
        {
            return new Promise<User>((resolve, reject) =>
            {
                string url = $"{BaseUrl}/by-token/{token}";

                RestClient.Get(url).Then(response =>
                {
                    User user = string.IsNullOrWhiteSpace(response.Text) ? null : JsonConvert.DeserializeObject<User>(response.Text);
                    
                    resolve(user);
                }).Catch(exception =>
                {
                    var error = exception as RequestException;
                    
                    if (error?.StatusCode != StatusCodes.NotFoundStatusCode)
                    {
                        resolve(null);
                    }
                    
                    reject(new Exception($"Error retrieving user by Token: {exception.Message}")); 
                });
            });
        }

        public static Promise<User> GetUserById(int id)
        {
            return new Promise<User>((resolve, reject) =>
            {
                string url = $"{BaseUrl}/{id}";

                RestClient.Get(url).Then(response =>
                {
                    User user = string.IsNullOrWhiteSpace(response.Text) ? null : JsonConvert.DeserializeObject<User>(response.Text);
                    
                    resolve(user);
                }).Catch(exception =>
                {
                    var error = exception as RequestException;
                    
                    if (error?.StatusCode != StatusCodes.NotFoundStatusCode)
                    {
                        resolve(null);
                    }
                    
                    reject(new Exception($"Error retrieving user by Id: {exception.Message}")); 
                });
            });
        }
        public static IPromise<int> SaveUser(UserRequest user)
        {
            var promise = new Promise<int>();

            if (string.IsNullOrEmpty(user.token) || string.IsNullOrEmpty(user.userName))
            {
                promise.Reject(new Exception("UserID or UserName is null or empty"));
                return promise;
            }

            RestClient.Post(BaseUrl, user).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, int>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("id", out int newUserId))
                {
                    promise.Resolve(newUserId);
                }
                else
                {
                    promise.Reject(new Exception("User ID not returned from API"));
                }
            }).Catch(error => { promise.Reject(error); });


            return promise;
        }

        public static IPromise<ResponseHelper> UpdateUserInfo(User user)
        {
            string keyUrlPart = $"{BaseUrl}/{user.id}";
            return RestClient.Put(keyUrlPart, user);
        }

        public static Promise<double> GetUserBalanceById(int userId)
        {
            return new Promise<double>((resolve, reject) =>
            {
                string queryUrl = $"{BaseUrl}/{userId}/balance";

                RestClient.Get(queryUrl).Then(response =>
                {
                    double.TryParse(response.Text, out double balance);

                    resolve(balance);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving user balance by UserID: {error.Message}"));
                });
            });
        }

        public static Promise<List<User>> GetAllUsers()
        {
            return new Promise<List<User>>((resolve, reject) =>
            {
                RestClient.Get(BaseUrl).Then(response =>
                {
                    if (string.IsNullOrEmpty(response.Text))
                    {
                        resolve(new List<User>());
                    }
                    
                    List<User> users = JsonConvert.DeserializeObject<List<User>>(response.Text);

                    resolve(users);
                }).Catch(error => { reject(new Exception($"Error retrieving all users: {error.Message}")); });
            });
        }
    }
}