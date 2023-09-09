using System;
using System.Collections.Generic;
using Proyecto26;
using RSG;
using UnityEngine;

namespace Libs.Helpers
{
    public static class ImageHelper
    {
        private const string FirebaseStorageURL = "https://firebasestorage.googleapis.com/v0/b/wwe-bets.appspot.com";

        public static Promise<string> UploadImage(Texture2D imageToUpload, string fileName)
        {
            return new Promise<string>((resolve, reject) =>
            {
                byte[] imageBytes = imageToUpload.EncodeToPNG();

                var headers = new Dictionary<string, string>
                {
                    { "Content-Type", "image/png" }
                };

                var requestData = new RequestHelper
                {
                    Uri = $"{FirebaseStorageURL}/o?uploadType=media&name={fileName}",
                    Method = "POST",
                    BodyRaw = imageBytes,
                    Headers = headers
                };

                RestClient.Request(requestData).Then(response =>
                {
                    GetDownloadURL(fileName).Then(resolve).Catch(error =>
                    {
                        reject(new Exception($"Error retrieving download URL: {error.Message}"));
                    });
                }).Catch(error => { reject(new Exception($"Error uploading image: {error.Message}")); });
            });
        }

        private static Promise<string> GetDownloadURL(string fileName)
        {
            return new Promise<string>((resolve, reject) =>
            {
                RestClient.Get($"{FirebaseStorageURL}/o/{fileName}").Then(response =>
                {
                    string downloadUrl = JsonUtility.FromJson<DownloadUrlResponse>(response.Text).downloadTokens;
                    resolve(downloadUrl);
                }).Catch(error => { reject(new Exception($"Error retrieving download URL: {error.Message}")); });
            });
        }

        [Serializable]
        public class DownloadUrlResponse
        {
            public string downloadTokens;
        }
    }
}