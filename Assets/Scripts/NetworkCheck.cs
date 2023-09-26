using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class NetworkCheck : MonoBehaviour
{
    public static bool IsNetworkActive;
    [SerializeField] private GameObject networkErrorPanel;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private PostProcessingController postProcessingController;
    public static Action OnInternetEstablished;

    private void Awake()
    {
        tryAgainButton.onClick.AddListener(CheckInternet);
        StartCoroutine(InternetCoroutine());
    }

    private IEnumerator InternetCoroutine()
    {
        while (true) 
        {
            CheckInternet();
            yield return new WaitForSeconds(10f); 
        }
    }

    private void CheckInternet()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            IsNetworkActive = false;
            networkErrorPanel.SetActive(true);
            postProcessingController.ToggleEffectsOnInternetConnection(true);
            return;
        }

        if (IsNetworkActive) return;
        networkErrorPanel.SetActive(false);
        IsNetworkActive = true;
        postProcessingController.ToggleEffectsOnInternetConnection(false);
        OnInternetEstablished?.Invoke();
    }
}
