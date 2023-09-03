using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class NetworkCheck : MonoBehaviour
{
    private static bool _isNetworkActive;
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
            _isNetworkActive = false;
            networkErrorPanel.SetActive(true);
            postProcessingController.ToggleEffectsOnInternetConnection(true);
            return;
        }

        if (_isNetworkActive) return;
        networkErrorPanel.SetActive(false);
        _isNetworkActive = true;
        postProcessingController.ToggleEffectsOnInternetConnection(false);
        OnInternetEstablished?.Invoke();
    }
}
