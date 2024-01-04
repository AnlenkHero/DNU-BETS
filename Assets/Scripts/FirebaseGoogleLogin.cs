using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using Libs.Helpers;
using Libs.Models.RequestModels;
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
    [SerializeField] private RawImage profileImage;
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
 /*       configuration = new GoogleSignInConfiguration
            { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
        SignInWithGoogle();*/

             var user = new UserRequest() { userId = "116993585815267308373", userName = "N", balance = 1000};
                 UserRepository.GetUserByUserId(user.userId).Then(userId =>
                 {
                     UserData.Balance = userId.balance;
                     UserData.Name = userId.userName;
                     UserData.UserId = user.userId;
                     OnLoginFinished?.Invoke();
                 }).Catch(errorUser =>
                 {
                     UserRepository.SaveUser(user).Then(userId =>
                     {
                         UserData.Name = user.userName;
                         UserData.Balance = user.balance;
                         UserData.UserId = user.userId;
                         OnLoginFinished?.Invoke();
                     }).Catch(error => { Debug.LogError(error.Message); });
                 });
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
    

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
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
            var result = task.Result;
            var user = new UserRequest()
            {
                userId = result.UserId, userName = result.DisplayName, balance = 1000,
                imageUrl = result.ImageUrl.ToString()
            };
            UserRepository.GetUserByUserId(user.userId).Then(userId =>
            {
                UserData.Balance = userId.balance;
                UserData.Name = userId.userName;
                UserData.UserId = user.userId;
                userId.imageUrl = user.imageUrl;
                userId.userName = user.userName;
                UserRepository.UpdateUserInfo(userId);
                OnLoginFinished?.Invoke();
            }).Catch(errorUser =>
            {
                UserRepository.SaveUser(user).Then(userId =>
                {
                    UserData.Name = user.userName;
                    UserData.Balance = user.balance;
                    UserData.UserId = user.userId;
                    OnLoginFinished?.Invoke();
                }).Catch(error => { Debug.LogError(error.Message); });
            });

            TextureLoader.LoadTexture(this, user.imageUrl, texture2D =>
            {
                if (texture2D != null)
                {
                    profileImage.texture = texture2D;
                }
                else
                {
                    Debug.Log("Texture failed to load.");
                }
            });
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

    private void AddToInformation(string str)
    {
        statusText.text += "\n" + str;
    }
}