using System;
using System.Collections.Generic;
using Libs.Config;
using Libs.Models;
using Newtonsoft.Json;
using Proyecto26;
using RSG;

namespace Libs.Repositories
{
    public static class BuffPurchasesRepository
    {
        private static readonly string BaseUrl = $"{ConfigManager.Settings.ApiSettings.Url}/api/BuffPurchase";
        
        public static IPromise<int> CreatePurchase(BuffPurchase buffPurchase)
        {
            var promise = new Promise<int>();

            RestClient.Post(BaseUrl, buffPurchase).Then(response =>
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
    }
}