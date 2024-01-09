using System;
using JetBrains.Annotations;
using Libs.Helpers;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Transform buttonsGrid;
    [SerializeField] private EmptyButton emptyButton;

    public static InfoPanel Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("More than one InfoPanel instance found!");
            return;
        }
        Instance = this;
    }

    public static void ShowPanel(Color color, [CanBeNull] string info = null,[CanBeNull] Action callback = null)
    {
        if (Instance == null)
        {
            Debug.LogError("No InfoPanel instance available!");
            return;
        }

        Instance.DisplayPanel(color, info, callback);
    }

    private void DisplayPanel(Color color, string info, Action callback)
    {
        buttonsGrid.ClearExistingElementsInParent();
        infoText.color = color;
        infoText.text = info ?? "";
        panel.SetActive(true);
        if (callback == null)
        {
            AddButton("Close", HidePanel);
        }
        callback?.Invoke();
    }
    
    public void AddButton(string buttonText, [CanBeNull] Action buttonAction = null, [CanBeNull] string buttonColorString = null)
    {
        var button = Instantiate(emptyButton, buttonsGrid);
        button.SetData(buttonText,buttonAction, buttonColorString);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }
}