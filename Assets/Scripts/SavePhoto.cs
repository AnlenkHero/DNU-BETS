using System;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Events;


public class SavePhoto : MonoBehaviour
{
    private string _finalPath;
    public UnityEvent Function_onSaved_Return;

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
                Variables.ActiveScene.Set("Picked File", _finalPath);
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

                Function_onSaved_Return.Invoke(); // Triggered [On Unity Event] in Visual Scripting
            }

        });

    }
    

    public static Texture2D GetTexture2DIOS(string path)
    {
        Texture2D newText_ = NativeGallery.LoadImageAtPath(path, -1, false, true, false);
        return newText_;
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
