using System;
using System.Collections;
using System.Collections.Generic;
using Libs.Config;
using Proyecto26;
using UnityEngine;

public class ApiAuthManager : MonoBehaviour
{
    private const int SecondsBeforeReaAuth = 5;
    
    public static Action OnApiAuthenticated;
    
    private void Awake()
    {
        ApiSettings apiSettings = ConfigManager.Settings.ApiSettings;
        
        if (apiSettings.UseAuthentication())
        {
            AuthenticateUser(apiSettings);
            StartCoroutine(StartReAuthenticationTimer(apiSettings));
            return;
        }
        
        OnApiAuthenticated?.Invoke();
    }
    
    private void AuthenticateUser(ApiSettings apiSettings)
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
            OnApiAuthenticated.Invoke();
        }).Catch(e =>
        {
            Debug.LogError(e.Message);
            OnApiAuthenticated.Invoke();
        });
    }
    
    private IEnumerator StartReAuthenticationTimer(ApiSettings apiSettings)
    {
        while (true)
        {
            yield return new WaitForSeconds(apiSettings.TokenLifeTimeInSeconds - SecondsBeforeReaAuth);
            AuthenticateUser(apiSettings);
        }
    }
}
