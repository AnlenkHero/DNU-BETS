using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using Libs.Helpers;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseGoogleLogin : MonoBehaviour
{
    public string webClientId = "1062902385276-u12o6kiqrmjcssl54u5i2n4cg17orqqb.apps.googleusercontent.com";

    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject loginPanel;

    private FirebaseAuth _auth;
    private GoogleSignInConfiguration _configuration;
    private bool _isSignInInProgress;
    public static Action OnLoginFinished;
    public static Action OnUserTextureLoadingFinished;

    private void OnEnable()
    {
        NetworkCheck.OnInternetEstablished += SignInWithGoogle;
        UserProfileManager.OnSignOut += OnSignOut;

//         LogIn();
        DebugLogIn();
    }


    private void DebugLogIn()
    {
        var user = new UserRequest() { userId = "116993585815267308373", userName = "N", balance = 1000 };
        UserRepository.GetUserByUserId(user.userId).Then(userId =>
        {
            UserData.Balance = userId.balance;
            UserData.Name = userId.userName;
            UserData.UserId = user.userId;
            LoadUserImage(userId.imageUrl);

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

    private void LogIn()
    {
        _configuration = new GoogleSignInConfiguration
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
                    _auth = FirebaseAuth.DefaultInstance;
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
        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();

        UserData.ClearUserData();

        loginPanel.SetActive(true);
        InfoPanelManager.Instance.HidePanel();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        _isSignInInProgress = false;

        if (task.IsFaulted)
        {
            HandleSignInFault(task);
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
        }
        else
        {
            HandleSuccessfulSignIn(task.Result);
        }
    }

    private void HandleSignInFault(Task<GoogleSignInUser> task)
    {
        if (task.Exception?.InnerExceptions.FirstOrDefault() is GoogleSignIn.SignInException error)
        {
            AddToInformation("Got Error: " + error.Status + " " + error.Message);
        }
        else
        {
            AddToInformation("Got Unexpected Exception?!?" + task.Exception);
        }
    }

    private void HandleSuccessfulSignIn(GoogleSignInUser result)
    {
        var user = CreateUserRequest(result);
        UpdateOrSaveUserData(user);
        SignInWithGoogleOnFirebase(result.IdToken);
    }

    private UserRequest CreateUserRequest(GoogleSignInUser result)
    {
        return new UserRequest
        {
            userId = result.UserId,
            userName = result.DisplayName,
            balance = 0,
            imageUrl = result.ImageUrl.ToString()
        };
    }

    private void UpdateOrSaveUserData(UserRequest user)
    {
        UserRepository.GetUserByUserId(user.userId)
            .Then(userId =>
            {
                LoadUserImage(userId.imageUrl);
                UpdateUserData(userId);
                OnLoginFinished?.Invoke();
                loginPanel.SetActive(false);
            })
            .Catch(errorUser =>
            {
                LoadUserImage(user.imageUrl);
                SaveNewUser(user);
            });
    }

    private void UpdateUserData(User userId)
    {
        UserData.Balance = userId.balance;
        UserData.Name = userId.userName;
        UserData.UserId = userId.userId;
        UserRepository.UpdateUserInfo(userId);
    }

    private void SaveNewUser(UserRequest user)
    {
        AppSettingsRepository.GetAppSettings()
            .Then(settings =>
            {
                user.balance = settings.defaultBalance;

                UserRepository.SaveUser(user)
                    .Then(userId =>
                    {
                        UserData.Name = user.userName;
                        UserData.Balance = user.balance;
                        UserData.UserId = user.userId;
                        OnLoginFinished?.Invoke();
                        loginPanel.SetActive(false);
                    })
                    .Catch(error => Debug.LogError(error.Message));
            }).Catch(exception => AddToInformation($"Failed to get AppSettings {exception}"));
    }

    private void LoadUserImage(string imageUrl)
    {
        TextureLoader.LoadTexture(this, imageUrl, texture2D =>
        {
            if (texture2D != null)
            {
                UserData.ProfileImage = texture2D;
                OnUserTextureLoadingFinished?.Invoke();
            }
            else
            {
                Debug.Log("Texture failed to load.");
            }
        });
    }


    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                AddFirebaseSignInError(task.Exception);
            }
            else
            {
                AddToInformation("Sign In Successful.");
            }
        });
    }

    private void AddFirebaseSignInError(AggregateException exception)
    {
        if (exception.InnerExceptions[0] is FirebaseException firebaseEx && firebaseEx.ErrorCode != 0)
        {
            AddToInformation($"Firebase SignIn Error - Code: {firebaseEx.ErrorCode}, Message: {firebaseEx.Message}");
        }
    }

    private void AddToInformation(string str)
    {
        statusText.text += "\n" + str;
    }
}