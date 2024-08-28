using System.Collections.Generic;
using System.Linq;
using Libs.Helpers;
using Libs.Models;
using UnityEngine;


public class MatchManager : MonoBehaviour
{
    [SerializeField] private Transform matchPanelParent;
    [SerializeField] private MatchView matchPanelPrefab;
    [SerializeField] private GameObject noMatchesPanel;
    [SerializeField] private GameObject matchPanelSkeletonLoading;
    private IEnumerable<Match> _availableMatches;

    private void OnEnable()
    {
        DataFetcher.OnFetchDataStarted += PrepareToRefresh;
        DataFetcher.OnFetchDataFinished += OnDataReceived;
    }

    private void PrepareToRefresh()
    {
        noMatchesPanel.SetActive(false);
        matchPanelSkeletonLoading.SetActive(true);
        matchPanelParent.ClearExistingElementsInParent();
    }

    private void OnDataReceived()
    {
        _availableMatches = MatchCache.Matches.Where(match => match.IsBettingAvailable);
        CreateMatchViews(_availableMatches.ToList());
        matchPanelSkeletonLoading.SetActive(false);
    }

    private void CreateMatchViews(List<Match> matches)
    {
        if (matches.Count == 0)
        {
            noMatchesPanel.SetActive(true);
            return;
        }

        foreach (var match in matches)
        {
            var matchView = Instantiate(matchPanelPrefab, matchPanelParent);

            matchView.SetData(match);
        }
    }
}