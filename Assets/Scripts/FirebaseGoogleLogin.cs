using System;
using System.Collections.Generic;
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

    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private Button profileImageButton;
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
        
        profileImageButton.onClick.AddListener(ShowProfilePanel);

        /* var user = new UserRequest() { userId = "116993585815267308373", userName = "N", balance = 1000};
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
              });*/
    }

    private void ShowProfilePanel()
    {
        InfoPanel.ShowPanel(Color.white, callback: () =>
        {
            InfoPanel.Instance.AddButton("Sign out", OnSignOut);
            InfoPanel.Instance.AddButton("Close", InfoPanel.Instance.HidePanel);
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

    private void OnSignOut() 
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
        
        UserData.UserId = "";
        UserData.Balance = 0;
        UserData.Name = "";
        
        loginPanel.SetActive(true);
        InfoPanel.Instance.HidePanel();
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
        LoadUserProfileImage(user.imageUrl);
        SignInWithGoogleOnFirebase(result.IdToken);
    }

    private UserRequest CreateUserRequest(GoogleSignInUser result)
    {
        return new UserRequest
        {
            userId = result.UserId,
            userName = result.DisplayName,
            balance = 1000,
            imageUrl = result.ImageUrl.ToString()
        };
    }

    private void UpdateOrSaveUserData(UserRequest user)
    {
        UserRepository.GetUserByUserId(user.userId)
            .Then(userId =>
            {
                UpdateUserData(user, userId);
                OnLoginFinished?.Invoke();
                loginPanel.SetActive(false);
            })
            .Catch(errorUser => { SaveNewUser(user); });
    }

    private void UpdateUserData(UserRequest user, User userId)
    {
        UserData.Balance = userId.balance;
        UserData.Name = userId.userName;
        UserData.UserId = user.userId;
        userId.imageUrl = user.imageUrl;
        userId.userName = user.userName;
        UserRepository.UpdateUserInfo(userId);
    }

    private void SaveNewUser(UserRequest user)
    {
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
    }

    private void LoadUserProfileImage(string imageUrl)
    {
        TextureLoader.LoadTexture(this, imageUrl, texture2D =>
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