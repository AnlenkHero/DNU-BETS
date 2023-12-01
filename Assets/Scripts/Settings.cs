using UnityEngine;

public class Settings : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }

}
