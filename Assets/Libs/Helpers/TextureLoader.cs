using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Libs.Helpers
{
    public static class TextureLoader
    {
        public static void LoadTexture(MonoBehaviour caller, string path, Action<Texture2D> result)
        {
            caller.StartCoroutine(LoadTextureRoutine(path, result));
        }

        private static IEnumerator LoadTextureRoutine(string path, Action<Texture2D> result)
        {
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load texture from \"{path}\": {uwr.error}");
                result(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                result(texture);
            }
        }
    }

}