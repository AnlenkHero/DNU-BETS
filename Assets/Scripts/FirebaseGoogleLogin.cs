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
    [SerializeField] private RawImage profileImage;
    [SerializeField] private Button profileImageButton;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private NameChanger nameChangerElement;

    private FirebaseAuth _auth;
    private GoogleSignInConfiguration _configuration;
    private bool _isSignInInProgress;
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
        GalleryFileManager.FunctionOnPickedFileReturn += ChangePhoto;
        profileImageButton.onClick.AddListener(ShowProfilePanel);

        //LogIn();
        DebugLogIn();
    }

    private static void DebugLogIn()
    {
        var user = new UserRequest() { userId = "116993585815267308373", userName = "N", balance = 1000 };
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

    private void LogIn()
    {
        _configuration = new GoogleSignInConfiguration
            { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
        SignInWithGoogle();
    }

    private void ChangePhoto(string path)
    {
        Texture2D originalTexture = GalleryFileManager.GetTexture2DIOS(path);
        Texture2D resizedTexture = ImageProcessing.ResizeAndCompressTexture(originalTexture, 600, 600, 100);

        ImageCropperNamespace.ImageCropper.Crop(resizedTexture, (croppedTexture) =>
        {
            Texture2D readableTexture = croppedTexture;

            profileImage.texture = readableTexture;
            UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
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

        UserData.UserId = "";
        UserData.Balance = 0;
        UserData.Name = "";
        profileImage.texture = null;

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
                LoadUserProfileImage(userId.imageUrl);
                UpdateUserData(userId);
                OnLoginFinished?.Invoke();
                loginPanel.SetActive(false);
            })
            .Catch(errorUser =>
            {
                LoadUserProfileImage(user.imageUrl);
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