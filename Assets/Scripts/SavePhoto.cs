using System;
using UnityEngine;


public class SavePhoto : MonoBehaviour
{
    private string _finalPath;
    public event  Action Function_onSaved_Return;

    private NativeGallery.Permission _permissionGal;
    public static Action<string> FunctionOnPickedFileReturn;

    public static SavePhoto Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("More than one InfoPanel instance found!");
            return;
        }
        Instance = this;
    }

    public static void PickPhoto()
    {
        Instance.PickPhotoGallery();
    }

    private void PickPhotoGallery()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(path =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                _finalPath = path;
                FunctionOnPickedFileReturn?.Invoke(_finalPath);
            }
        });
    }
	
    public void SavePhotoToCameraRoll(Texture2D MyTexture, string AlbumName, string filename)
    {
        NativeGallery.SaveImageToGallery(MyTexture, AlbumName, filename, (callback, path) =>
        {
            if (callback == false)
            {
                Debug.Log("Failed to save !");
            }
            else
            {
                Debug.Log("Photo is saved to Camera Roll on phone device.");

                Function_onSaved_Return?.Invoke(); 
            }

        });

    }
    

    public static Texture2D GetTexture2DIOS(string path)
    {
        Texture2D newText_ = NativeGallery.LoadImageAtPath(path, -1, false, true, false);
        return newText_;
    }

    public static Texture2D ResizeTexture(Texture2D texture2D, int maxWidth, int maxHeight)
    {
        TextureScale.Bilinear(texture2D, maxWidth, maxHeight);
        return texture2D;
    }


    public async void AskPermissionGal()
    {
        NativeGallery.Permission permissionResultGal = await NativeGallery.RequestPermissionAsync(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        _permissionGal = permissionResultGal;

        if (_permissionGal == NativeGallery.Permission.Granted)
        {
            PickPhotoGallery();
        }
    }
    

}
