using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollBar;
    private float _scrollPos = 0;
    private Dictionary<float,List<BetButtonView>> _posMatch;
    public static List<BetButtonView> SelectedMatchButtons { get; private set; }
    private float[] _pos;
    
    

    private void InitializePositions()
    {
        _posMatch = new();
        _pos = new float [transform.childCount];
        MatchView[] views =transform.GetComponentsInChildren<MatchView>(); //TODO unreal low lvl to have it in update
        float distance = 1f / (views.Length - 1f);
        for (int i = 0; i < views.Length; i++)
        {
            _pos[i] = distance * i;
            _posMatch[distance * i] = views[i].buttonViews;
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
        for (int i = 0; i < _posMatch.Count; i++)
        {
            if (IsWithinDistance(_scrollPos, _pos[i], distance))
            {
                SelectedMatchButtons=_posMatch[_pos[i]];
                scrollBar.value = Mathf.Lerp(scrollBar.value, _pos[i], 0.05f);
            }
        }
    }

    private bool IsWithinDistance(float value, float target, float distance)
    {
        return value < target + (distance / 2) && value > target - (distance / 2);
    }
}