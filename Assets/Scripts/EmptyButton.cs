using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmptyButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;

    public void SetData(string textForButton, [CanBeNull] Action buttonAction = null, [CanBeNull] string buttonColorString = null)
    {
        buttonText.text = textForButton;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            buttonAction?.Invoke();
        });
        
        if (!ColorUtility.TryParseHtmlString(buttonColorString, out var newColor))
        {
            ColorUtility.TryParseHtmlString("#ffffff", out newColor);
        }

        buttonImage.color = newColor;
    }

}