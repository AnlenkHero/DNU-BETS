using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using Libs.Models;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseGoogleLogin : MonoBehaviour
{
    public string webClientId = "1062902385276-u12o6kiqrmjcssl54u5i2n4cg17orqqb.apps.googleusercontent.com";

    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image profileImage;
    [SerializeField] private GameObject loginPanel;
    private bool _isSignInInProgress = false;
    public static Action OnLoginFinished;

    private void OnEnable()
    {
        NetworkCheck.OnInternetEstablished += SignInWithGoogle;
    }

    private void OnDisable()
    {
        NetworkCheck.OnInternetEstablished -= SignInWithGoogle;
    }

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration
            { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
        SignInWithGoogle();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    auth = FirebaseAuth.DefaultInstance;
                else
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                AddToInformation("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    public void SignInWithGoogle()
    {
        if (_isSignInInProgress)
        {
            return;
        }

        _isSignInInProgress = true;
        OnSignIn();
    }

    public void SignOutFromGoogle()
    {
        OnSignOut();
    }

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddToInformation("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            _isSignInInProgress = false;
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            _isSignInInProgress = false;
            AddToInformation("Canceled");
        }
        else
        {
            // AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            //AddToInformation("Email = " + task.Result.Email);
            // name.text = task.Result.DisplayName;
            //AddToInformation("Google ID Token = " + task.Result.IdToken);
            //AddToInformation("Email = " + task.Result.Email);
            var result = task.Result;
            var user = new User() { userId = result.UserId, userName = result.DisplayName, Balance = 1000};
            UserRepository.GetUserByUserId(user.userId).Then(userId =>
            {
                UserData.Balance = userId.Balance;
                UserData.Name = userId.userName;
                UserData.UserId = user.userId;
                OnLoginFinished?.Invoke();
            }).Catch(errorUser =>
            {
                UserRepository.SaveUser(user).Then(userId =>
                {
                    UserData.Name = user.userName;
                    UserData.Balance = user.Balance;
                    UserData.UserId = user.userId;
                    OnLoginFinished?.Invoke();
                }).Catch(error => { Debug.LogError(error.Message); });
            });
            ;
            //nameText.text = result.DisplayName;
            StartCoroutine(LoadImage(result.ImageUrl.ToString()));
            loginPanel.SetActive(false);
            OnLoginFinished?.Invoke();
            SignInWithGoogleOnFirebase(result.IdToken);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            // check for error.
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                    AddToInformation("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            }
            else
            {
                AddToInformation("Sign In Successful.");
            }
        });
    }

    // these 2 functions are currently not used in this demo. but you can use it as per your need.
    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddToInformation("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void AddToInformation(string str)
    {
        statusText.text += "\n" + str;
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        WWW www = new WWW(imageUrl);
        yield return www;

        profileImage.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height),
            new Vector2(0, 0));
    }
}