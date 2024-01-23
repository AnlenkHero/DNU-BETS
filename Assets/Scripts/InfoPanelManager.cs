using System;
using JetBrains.Annotations;
using Libs.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Transform buttonsGrid;
    [SerializeField] private EmptyButton emptyButton;
    [SerializeField] private RawImage panelImage;
    public Transform createdElementsParent;

    public static InfoPanelManager Instance;

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
        createdElementsParent.ClearExistingElementsInParent();
        buttonsGrid.ClearExistingElementsInParent();
        infoText.color = color;
        infoText.text = info ?? "";
        panel.SetActive(true);
        DeleteImage();
        
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

    public void SetImage(Texture texture)
    {
        panelImage.texture = texture;
        panelImage.gameObject.SetActive(true);
        infoText.rectTransform.SetTop(panelImage.rectTransform.rect.height);
    }

    private void DeleteImage()
    {
        panelImage.texture = null;
        panelImage.gameObject.SetActive(false);
        infoText.rectTransform.SetTop(0);
    }
    public void HidePanel()
    {
        panel.SetActive(false);
    }
}