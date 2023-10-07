using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button button;

    private static InfoPanel _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("More than one InfoPanel instance found!");
            return;
        }
        _instance = this;
    }

    public static void ShowPanel(Color color, [CanBeNull] string info = null, [CanBeNull] string textForButton = "Close",[CanBeNull] Action buttonAction = null)
    {
        if (_instance == null)
        {
            Debug.LogError("No InfoPanel instance available!");
            return;
        }

        _instance.DisplayPanel(color, textForButton, info, buttonAction);
    }

    private void DisplayPanel(Color color, string textForButton, string info, Action buttonAction)
    {
        infoText.color = color;
        infoText.text = info ?? "";
        buttonText.text = textForButton ?? "hide";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            buttonAction?.Invoke();
        });
        panel.SetActive(true);
    }
}