using Libs.Helpers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PressedButtonBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler 
{
    [SerializeField] private RectTransform movingPartRectTransform;
    [SerializeField] private Image staticButtonImage;
    [SerializeField] private Image movingPartImage;
    [SerializeField] private Vector2 desiredMovingPosition;
    private Vector2 _movingPartInitialPosition;

    private bool _isButtonEnabled = true;

    protected void Awake()
    {
        _movingPartInitialPosition = movingPartRectTransform.anchoredPosition;
        movingPartImage.color = staticButtonImage.color;
    }

    public void DisableButton()
    {
        _isButtonEnabled = false;
        movingPartRectTransform.anchoredPosition = desiredMovingPosition;
        movingPartImage.color = ColorHelper.PastelGray;
    }

    public void EnableButton()
    {
        _isButtonEnabled = true;
        movingPartRectTransform.anchoredPosition = _movingPartInitialPosition;
        movingPartImage.color = staticButtonImage.color;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isButtonEnabled)
        {
            movingPartRectTransform.anchoredPosition = desiredMovingPosition;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isButtonEnabled)
        {
            movingPartRectTransform.anchoredPosition = _movingPartInitialPosition;
        }
    }
}