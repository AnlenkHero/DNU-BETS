using System;
using System.Net;
using Firebase;
using Firebase.Auth;
using Google;
using Libs.Config;
using Libs.Helpers;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using RSG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseGoogleLogin : MonoBehaviour
{
    private const string WebClientId = "1062902385276-u12o6kiqrmjcssl54u5i2n4cg17orqqb.apps.googleusercontent.com";

    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private Button profileImageButton;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private NameChanger nameChangerElement;

    private FirebaseAuth _auth;
    private GoogleSignInConfiguration _configuration;
    private bool _isSignInInProgress;
    public static Action OnLoginFinished;

    private void Awake()
    {
        GalleryFileManager.FunctionOnPickedFileReturn += ChangePhoto;
        profileImageButton.onClick.AddListener(ShowProfilePanel);
        //ApiAuthManager.OnApiAuthenticated += LogIn;

        ApiAuthManager.OnApiAuthenticated += DebugLogIn;
    }

    private static void DebugLogIn()
    {
        var testUserRequest = new UserRequest() { token = "116993585815267308373", userName = "Nigger", balance = 1000 };
        UserRepository.GetUserByToken(testUserRequest.token).Then(user =>
        {
            if (user == null)
            {
                UserRepository.SaveUser(testUserRequest).Then(userId =>
                {
                    SetUserData(testUserRequest, userId);
                    ApiAuthManager.AuthenticateUserByToken(testUserRequest.token)
                        .Then(() => OnLoginFinished?.Invoke())
                        .Catch(e =>
                        {
                            Debug.LogError(e.Message);
                            OnLoginFinished?.Invoke(); 
                        });
                }).Catch(error => { Debug.LogError(error.Message); });
                return;
            }
            
            SetUserData(user);
            ApiAuthManager.AuthenticateUserByToken(testUserRequest.token)
                .Then(() => OnLoginFinished?.Invoke())
                .Catch(e =>
                {
                    Debug.LogError(e.Message);
                    OnLoginFinished?.Invoke(); 
                });
        }).Catch(error => { Debug.LogError($"Error while logging in with debug {error.Message}"); });
    }

    private void LogIn()
    {
        NetworkCheck.OnInternetEstablished += SignInWithGoogle;
        _configuration = new GoogleSignInConfiguration
            { WebClientId = WebClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
        SignInWithGoogle();
    }

    private static void SetUserData(UserRequest user, int userId)
    {
        UserData.Name = user.userName;
        UserData.Balance = user.balance;
        UserData.UserId = userId;
    }
    
    private static void SetUserData(User user)
    {
        UserData.Name = user.userName;
        UserData.Balance = user.balance;
        UserData.UserId = user.id;
    }
    
    private void ChangePhoto(string path)
    {
        Texture2D originalTexture = GalleryFileManager.GetTexture2DIOS(path);
        Texture2D resizedTexture = ImageProcessing.ResizeAndCompressTexture(originalTexture, 600, 600, 100);

        ImageCropperNamespace.ImageCropper.Crop(resizedTexture, (croppedTexture) =>
        {
            Texture2D readableTexture = croppedTexture;

            profileImage.texture = readableTexture;
            UserRepository.GetUserById(UserData.UserId).Then(user =>
            {
                ImageHelper.UploadImage(readableTexture, $"{Guid.NewGuid()}.png").Then(imageUrl =>
                {
                    ImageHelper.DeleteImage(user.imageUrl).Finally(() =>
                    {
                        user.imageUrl = imageUrl;
                        UserRepository.UpdateUserInfo(user)
                            .Catch(Debug.Log);
                    });
                }).Catch(Debug.Log);
            }).Catch(Debug.Log);
        });
    }

    private void ShowProfilePanel()
    {
        InfoPanelManager.ShowPanel(Color.white, callback: () =>
        {
            Instantiate(nameChangerElement, InfoPanelManager.Instance.createdElementsParent);
            InfoPanelManager.Instance.AddButton("Change photo", GalleryFileManager.PickPhoto, ColorHelper.PaleYellowString);
            InfoPanelManager.Instance.AddButton("Sign out", OnSignOut, ColorHelper.HotPinkString);
            InfoPanelManager.Instance.AddButton("Close", InfoPanelManager.Instance.HidePanel, ColorHelper.PaleYellowString);
        });
    }

    private void CheckFirebaseDependencies()
    {
        try
        {
            DependencyStatus dependencyStatus = FirebaseApp.CheckAndFixDependenciesAsync().GetAwaiter().GetResult();
            if (dependencyStatus == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                return;
            }

            AddToInformation($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
        catch (Exception e)
        {
            AddToInformation($"Dependency check was not completed. Error : {e.Message}");
        }
    }

    public void SignInWithGoogle()
    {
        if (_isSignInInProgress)
        {
            return;
        }
        
        OnSignIn();
    }


    private void OnSignIn()
    {
        _isSignInInProgress = true;
        
        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn");

        try
        {
            GoogleSignInUser googleUser = GoogleSignIn.DefaultInstance.SignIn().GetAwaiter().GetResult();
            _isSignInInProgress = false;
            HandleSuccessfulSignIn(googleUser);

        }
        catch (GoogleSignIn.SignInException e)
        {
            AddToInformation($"Got Error: {e.Status} {e.Message}");
        }
        catch (Exception e)
        {
            AddToInformation($"Got Unexpected Exception?!? {e.Message}");
        }
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();

        UserData.Balance = default;
        UserData.Name = default;
        profileImage.texture = null;

        loginPanel.SetActive(true);
        InfoPanelManager.Instance.HidePanel();
    }

    private void HandleSuccessfulSignIn(GoogleSignInUser result)
    {
        UserRequest request = CreateUserRequest(result);
        UpdateOrSaveUserData(request);
        SignInWithGoogleOnFirebase(result.IdToken);
    }

    private UserRequest CreateUserRequest(GoogleSignInUser result)
    {
        return new UserRequest
        {
            token = result.UserId,
            userName = result.DisplayName,
            balance = 0,
            imageUrl = result.ImageUrl.ToString()
        };
    }

    private void UpdateOrSaveUserData(UserRequest request)
    {
        LoadUserProfileImage(request.imageUrl);
        
        UserRepository.GetUserByToken(request.token)
            .Then(user =>
            {
                if (user == null)
                {
                    SaveNewUser(request);
                    return;
                }
                
                UpdateUserData(user).Then(() =>
                {
                    OnLoginFinished?.Invoke();
                    loginPanel.SetActive(false);
                }).Catch(err =>
                {
                    Debug.LogError(err.Message);
                    OnLoginFinished?.Invoke();
                    loginPanel.SetActive(false);
                });
            })
            .Catch(error=>
            {
                Debug.LogError($"Error authenticating user {error.Message}");
            });
    }

    private IPromise UpdateUserData(User user)
    {
        return new Promise((resolve, reject) =>
        {
            UserData.Balance = user.balance;
            UserData.Name = user.userName;

            // Authenticate the user by token
            ApiAuthManager.AuthenticateUserByToken(user.token)
                .Then(() =>
                {
                    UserRepository.UpdateUserInfo(user)
                        .Then(_ =>
                        {
                            resolve();
                        })
                        .Catch(error =>
                        {
                            reject(error);
                        });
                })
                .Catch(error =>
                {
                    reject(error);
                });
        });
    }

    private void SaveNewUser(UserRequest request)
    {
        request.balance = ConfigManager.Settings.DefaultBalance;

        UserRepository.SaveUser(request)
            .Then(userId =>
            {
                UserData.Name = request.userName;
                UserData.Balance = request.balance;
                UserData.UserId = userId;
                ApiAuthManager.AuthenticateUserByToken(request.token)
                    .Then(() => OnLoginFinished?.Invoke())
                    .Catch((err) =>
                    {
                        Debug.LogError(err.Message);
                    });
                loginPanel.SetActive(false);
            });
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

        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
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