using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ImageCropperNamespace
{
    public class ImageCropper : MonoBehaviour
    {
        public RawImage croppedImageHolder;
        [SerializeField] private bool ovalSelectionInput;
        [SerializeField] private bool autoZoomInput;
        [SerializeField] private float minAspectRatioInput;
        [SerializeField] private float maxAspectRatioInput;

        public static ImageCropper Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one InfoPanel instance found!");
                return;
            }
            Instance = this;
        }

        public static void Crop(Texture imageToCrop, Action<Texture2D> onCropComplete)
        {
            if (global::ImageCropper.Instance.IsOpen)
                return;

            Instance.StartCropCoroutine(imageToCrop, onCropComplete);
        }

        private void StartCropCoroutine(Texture imageToCrop, Action<Texture2D> onCropComplete)
        {
            StartCoroutine(TakeScreenshotAndCrop(imageToCrop, onCropComplete));
        }

        private IEnumerator TakeScreenshotAndCrop(Texture imageToCrop, Action<Texture2D> onCropComplete)
        {
            yield return new WaitForEndOfFrame();

            global::ImageCropper.Instance.Show(imageToCrop, (bool result, Texture originalImage, Texture2D croppedImage) =>
            {
                if (result)
                {
                    croppedImageHolder.texture = croppedImage;
                    onCropComplete?.Invoke(croppedImage);
                }
            }, settings: new global::ImageCropper.Settings()
            {
                ovalSelection = ovalSelectionInput,
                autoZoomEnabled = autoZoomInput,
                imageBackground = Color.clear,
                selectionMinAspectRatio = minAspectRatioInput,
                selectionMaxAspectRatio = maxAspectRatioInput
            }, croppedImageResizePolicy: (ref int width, ref int height) =>
            {
                // uncomment lines below to save cropped image at half resolution
                //width /= 2;
                //height /= 2;
            });
        }
    }
}
