using System;
using System.Collections;
using System.Collections.Generic;
using Libs.Config;
using Libs.Models;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using UnityEngine;

public class ApiAuthManager : MonoBehaviour
{
    private const int SecondsBeforeReaAuth = 5;
    public static Action OnApiAuthenticated;

    public static IPromise AuthenticateUserByToken(string token) //TODO somehow handle 401 after token expires
    {
        ApiSettings apiSettings = ConfigManager.Settings.ApiSettings;

        var requestHelper = new RequestHelper
        {
            Uri = $"{apiSettings.Url}/{apiSettings.LoginByTokenEndpoint}?token={token}"
        };

        return new Promise((resolve, reject) =>
        {
            RestClient.Post(requestHelper).Then(response =>
            {
                RestClient.DefaultRequestHeaders["Authorization"] = $"Bearer {response.Text}";
                
                resolve();
            }).Catch(error =>
            {
                reject(error);
            });
        });
    }
    
    private void Awake()
    {
        ApiSettings apiSettings = ConfigManager.Settings.ApiSettings;

        if (apiSettings.UseAuthentication())
        {
            AuthenticateMobileApp(apiSettings);
            StartCoroutine(StartReAuthenticationTimer(apiSettings));
            return;
        }

        OnApiAuthenticated?.Invoke();
    }

    private void AuthenticateMobileApp(ApiSettings apiSettings)
    {
        var requestHeaders = new Dictionary<string, string>
        {
            { "login", apiSettings.Login },
            { "password", apiSettings.Password }
        };

        var requestHelper = new RequestHelper
        {
            Uri = $"{apiSettings.Url}/{apiSettings.LoginEnpoint}",
            Headers = requestHeaders,
            ParseResponseBody = true
        };

        RestClient.Post(requestHelper).Then(response =>
        {
            RestClient.DefaultRequestHeaders["Authorization"] = $"Bearer {response.Text}";
            FetchAppSettings(apiSettings); 
        }).Catch(e =>
        {
            Debug.LogError(e.Message);
            OnApiAuthenticated?.Invoke(); 
        });
    }

    private void FetchAppSettings(ApiSettings apiSettings)
    {
        var requestHelper = new RequestHelper
        {
            Uri = $"{apiSettings.Url}/api/setting",
            Headers = RestClient.DefaultRequestHeaders
        };

        RestClient.Get(requestHelper).Then(response =>
        {
            AppSettingsModel settings = JsonConvert.DeserializeObject<AppSettingsModel>(response.Text);
            ConfigManager.Settings.DefaultBalance = settings?.DefaultBalance ?? ConfigManager.Settings.DefaultBalance;
            OnApiAuthenticated?.Invoke();
        }).Catch(e =>
        {
            var requestException = e as RequestException;
            
            OnApiAuthenticated?.Invoke();
            
            Debug.LogError($"Failed to fetch settings: {requestException.StatusCode}:{requestException.Message}");
        });
    }

    private IEnumerator StartReAuthenticationTimer(ApiSettings apiSettings)
    {
        while (true)
        {
            yield return new WaitForSeconds(apiSettings.TokenLifeTimeInSeconds - SecondsBeforeReaAuth);
            AuthenticateMobileApp(apiSettings);
        }
    }
}