using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class SkeletonAnimator : MonoBehaviour
{
    [SerializeField] [CanBeNull] private Image[] background;
    [SerializeField] private Image[] imagesArray;
    private readonly float duration = 1f;

    void OnEnable()
    {
        StartCoroutine(background != null ? AnimateGradientWithBackground() : AnimateGradientWithoutBackground());
    }

    IEnumerator AnimateGradientWithBackground()
    {
        float time = 0;

        while (true)
        {
            float lerpValue = Mathf.PingPong(time, duration);

            foreach (var image in imagesArray)
            {
                image.color = Color.Lerp(Color.black, Color.white, lerpValue);
            }

            if (background != null)
            {
                foreach (var element in background)
                {
                    element.color = Color.Lerp(Color.white, Color.black, lerpValue);
                }
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator AnimateGradientWithoutBackground()
    {
        float time = 0;

        while (true)
        {
            float lerpValue = Mathf.PingPong(time, duration);

            foreach (var image in imagesArray)
            {
                image.color = Color.Lerp(Color.black, Color.white, lerpValue);
            }

            time += Time.deltaTime;
            yield return null;
        }
    }
}