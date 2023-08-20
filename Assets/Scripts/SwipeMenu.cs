using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollBar;
    private float _scrollPos = 0;
    private Dictionary<float,List<BetButtonView>> _posMatch;
    public static List<BetButtonView> SelectedMatchButtons { get; private set; } = new List<BetButtonView>();
    private float[] _pos;
    private MatchView[] _views;

    public void InitializeViews()
    {
        _views =transform.GetComponentsInChildren<MatchView>();
    }

    private void InitializePositions()
    {
        _posMatch = new();
        _pos = new float [transform.childCount];
        
        float distance = 1f / (_pos.Length - 1f);
        for (int i = 0; i < _pos.Length; i++)
        {
            _pos[i] = distance * i;
            _posMatch[distance * i] = _views[i].buttonViews;
        }
    }
    
    private void Update()
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

    private void UpdateScrollPosition()
    {
        _scrollPos = scrollBar.value;
    }

    private void LerpToClosestPosition()
    {
        float distance = 1f / (_posMatch.Count - 1f);
        float closestPosition = _pos[0]; 
        float smallestDifference = Mathf.Abs(_scrollPos - closestPosition);

        for (int i = 0; i < _posMatch.Count; i++)
        {
            float currentDifference = Mathf.Abs(_scrollPos - _pos[i]);
            if (currentDifference < smallestDifference)
            {
                smallestDifference = currentDifference;
                closestPosition = _pos[i];
            }
        }

        SelectedMatchButtons = _posMatch[closestPosition];
        scrollBar.value = Mathf.Lerp(scrollBar.value, closestPosition, 0.1f);
    }

    private bool IsWithinDistance(float value, float target, float distance)
    {
        return value < target + (distance / 2) && value > target - (distance / 2);
    }
}