using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BetsHistorySkeletonAnimator : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image[] imagesArray;
    private readonly float duration = 1f;

    void OnEnable()
    {
        StartCoroutine(AnimateGradient());
    }

    IEnumerator AnimateGradient()
    {
        float time = 0;

        while (true)
        {
            float lerpValue = Mathf.PingPong(time, duration);

            foreach (var image in imagesArray)
            {
                image.color = Color.Lerp(Color.black, Color.white, lerpValue);   
            }
            
            background.color = Color.Lerp(Color.white,Color.black, lerpValue);

            time += Time.deltaTime;
            yield return null;
        }
    }
}