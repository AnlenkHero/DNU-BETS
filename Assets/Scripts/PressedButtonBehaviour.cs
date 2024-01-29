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

    protected void Start()
    {
        _movingPartInitialPosition = movingPartRectTransform.anchoredPosition;
        movingPartImage.color = staticButtonImage.color;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        movingPartRectTransform.anchoredPosition = desiredMovingPosition;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        movingPartRectTransform.anchoredPosition = _movingPartInitialPosition;
    }
}