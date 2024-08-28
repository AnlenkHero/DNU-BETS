using System;
using Libs.Helpers;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class UserProfileManager : MonoBehaviour
{
    [SerializeField] private RawImage profileImage;
    [SerializeField] private Button profileImageButton;
    [SerializeField] private NameChanger nameChangerElement;
    public static event Action OnSignOut;

    private void OnEnable()
    {
        GalleryFileManager.FunctionOnPickedFileReturn += ChangePhoto;
        FirebaseGoogleLogin.OnUserTextureLoadingFinished += () => SetProfilePhoto(UserData.ProfileImage);
        profileImageButton.onClick.AddListener(ShowProfilePanel);
    }

    private void ShowProfilePanel()
    {
        InfoPanelManager.ShowPanel(Color.white, callback: () =>
        {
            Instantiate(nameChangerElement, InfoPanelManager.Instance.createdElementsParent);
            InfoPanelManager.Instance.AddButton("Change photo", GalleryFileManager.PickPhoto,
                ColorHelper.PaleYellowString);
            InfoPanelManager.Instance.AddButton("Sign out", () => OnSignOut?.Invoke(), ColorHelper.HotPinkString);
            InfoPanelManager.Instance.AddButton("Close", InfoPanelManager.Instance.HidePanel,
                ColorHelper.PaleYellowString);
        });
    }

    private void ChangePhoto(string path)
    {
        Texture2D originalTexture = GalleryFileManager.GetTexture2DIOS(path);
        Texture2D resizedTexture = ImageProcessing.ResizeAndCompressTexture(originalTexture, 600, 600, 100);

        ImageCropperNamespace.ImageCropper.Crop(resizedTexture, croppedTexture =>
        {
            SetProfilePhoto(croppedTexture);
            UpdateUserImage(croppedTexture);
            UserData.ProfileImage = croppedTexture;
        });
    }

    private void UpdateUserImage(Texture2D texture)
    {
        UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
        {
            ImageHelper.UploadImage(texture, $"{Guid.NewGuid()}.png").Then(imageUrl =>
            {
                ImageHelper.DeleteImage(user.imageUrl).Finally(() =>
                {
                    user.imageUrl = imageUrl;
                    UserRepository.UpdateUserInfo(user).Catch(Debug.Log);
                });
            }).Catch(Debug.Log);
        }).Catch(Debug.Log);
    }

    private void SetProfilePhoto(Texture2D newTexture)
    {
        if (newTexture != null)
        {
            profileImage.texture = newTexture;
        }
        else
        {
            Debug.LogWarning("New texture is null, cannot set profile photo.");
        }
    }


}