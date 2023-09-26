using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollBar;
    private float _scrollPos = 0;
    private float[] _pos;

    private void InitializePositions()
    {
        _pos = new float [transform.childCount];
        float distance = 1f / (_pos.Length - 1f);
        for (int i = 0; i < _pos.Length; i++)
        {
            _pos[i] = distance * i;
        }
    }

    private void Update()
    {
        if (NetworkCheck.IsNetworkActive && DataMapper.MatchesAvailable)
        {
            InitializePositions();
            if (Input.GetMouseButton(0))
            {
                UpdateScrollPosition();
            }
            else
            {
                LerpToClosestPosition();
            }
        }
    }

    private void UpdateScrollPosition()
    {
        _scrollPos = scrollBar.value;
    }

    private void LerpToClosestPosition()
    {

        float closestPosition = _pos[0];
        float smallestDifference = Mathf.Abs(_scrollPos - closestPosition);

        for (int i = 0; i < _pos.Length; i++)
        {
            float currentDifference = Mathf.Abs(_scrollPos - _pos[i]);
            if (currentDifference < smallestDifference)
            {
                smallestDifference = currentDifference;
                closestPosition = _pos[i];
            }
        }


        scrollBar.value = Mathf.Lerp(scrollBar.value, closestPosition, 0.1f);
    }

    private bool IsWithinDistance(float value, float target, float distance)
    {
        return value < target + (distance / 2) && value > target - (distance / 2);
    }
}

/*  public ScrollRect ScrollRect;
  public RectTransform contentPanel;
  public RectTransform sampleListItem;
  public HorizontalLayoutGroup hlg;

  public float magnitudeTreshold = 1200f;
  public float snapSpeed;
  private float snapForce = 500;
  private bool isSnapped;

  private void Start()
  {
      isSnapped = false;
  }

  public void Update()
  {
      if (contentPanel.childCount > 0)
      {
          sampleListItem = contentPanel.GetChild(0).GetComponent<RectTransform>();
      }

      if (sampleListItem != null)
      {
          float itemWidth = sampleListItem.rect.width + hlg.spacing;
          int currentItem = Mathf.RoundToInt(contentPanel.localPosition.x / itemWidth);
          float targetPosition = currentItem * itemWidth;

          if (ScrollRect.velocity.magnitude < magnitudeTreshold && !isSnapped)
          {
              ScrollRect.velocity = Vector2.zero;
              snapSpeed += snapForce * Time.deltaTime;
              float newX = Mathf.MoveTowards(contentPanel.localPosition.x, targetPosition, snapSpeed);
              contentPanel.localPosition = new Vector3(newX, contentPanel.localPosition.y, contentPanel.localPosition.z);
              if (Mathf.Approximately(contentPanel.localPosition.x, targetPosition))
                  isSnapped = true;
          }

          if (ScrollRect.velocity.magnitude > magnitudeTreshold)
          {
              isSnapped = false;
              snapSpeed = 0;
          }
      }
  }
}*/
/*
 using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour, IEndDragHandler
{
    public ScrollRect scrollRect;
    public float animationTime = 0.1f;
    private float[] _positions;
    private int _currentPosition;

    private void Start()
    {
        UpdatePositions();
        scrollRect.horizontalNormalizedPosition = 0;
    }

    private void UpdatePositions()
    {
        int itemCount = scrollRect.content.childCount;
        _positions = new float[itemCount];
        float distance = 1f / (float)(itemCount - 1);
        for (int i = 0; i < itemCount; i++)
        {
            _positions[i] = distance * i;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UpdatePositions();
        float closest = float.MaxValue;
        for (int i = 0; i < _positions.Length; i++)
        {
            float distance = Mathf.Abs(_positions[i] - scrollRect.horizontalNormalizedPosition);
            if (distance < closest)
            {
                closest = distance;
                _currentPosition = i;
            }
        }
        StartCoroutine(SmoothMove(scrollRect.horizontalNormalizedPosition, _positions[_currentPosition], animationTime));
    }

    System.Collections.IEnumerator SmoothMove(float startpos, float endpos, float time)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            float newPos = Mathf.Lerp(startpos, endpos, (elapsedTime / time));
            scrollRect.horizontalNormalizedPosition = newPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = endpos;
    }
}
 */
