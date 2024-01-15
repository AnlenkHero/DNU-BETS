using System.Collections;
using System.Collections.Generic;
using Libs.Helpers;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetsHistory : MonoBehaviour
{
    [SerializeField] private BetHistoryElement betHistoryElement;
    [SerializeField] private Transform betHistoryParent;
    [SerializeField] private BetsHistoryTotalInfo betsHistoryTotalInfo;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject skeletonLoading;
    [SerializeField] private TextMeshProUGUI betHistoryErrorTMP;
    private bool _isBetsHistoryRefreshing;


    private void OnEnable()
    {
        DataMapper.OnMapDataStarted += StartLoading;
        DataMapper.OnMapDataFinished += FetchAndProcessBets;
        BetsController.OnBetPosted += FetchAndProcessBets;
    }

    private void StartLoading()
    {
        if (_isBetsHistoryRefreshing) return;
        betHistoryParent.ClearExistingElementsInParent();
        SetLoadingState(true);
    }

    private void FetchAndProcessBets()
    {
        if (MatchCache.Matches == null || BetCache.Bets == null)
        {
            SetLoadingState(false);
            HandleError("Failed to load matches ");
            return;
        }

        ProcessBets(BetCache.Bets, MatchCache.Matches);
    }


    private void ProcessBets(List<Bet> bets, List<Match> matches)
    {
        var betHistoryElements = new List<BetHistoryElement>();
        BetsProcessor.ProcessBets(bets, matches, stats =>
        {
            foreach (var processedBet in stats.ProcessedBets)
            {
                var tempBetHistoryElement = Instantiate(betHistoryElement, betHistoryParent);

                tempBetHistoryElement.SetData(processedBet);

                betHistoryElements.Add(tempBetHistoryElement);
            }
            
            SortBetsHistory(betHistoryElements);

            betsHistoryTotalInfo.SetData(bets.Count, stats.BetsWon, stats.BetsLost, stats.MoneyGained, stats.MoneyLost,
                stats.MatchesCanceled);

            SetLoadingState(false);
            StartCoroutine(ScrollToTop());
        });
    }

    private static void SortBetsHistory(List<BetHistoryElement> betHistoryElements)
    {
        betHistoryElements.Sort((a, b) => b.Date.CompareTo(a.Date));
        for (var i = 0; i < betHistoryElements.Count; i++)
        {
            betHistoryElements[i].transform.SetSiblingIndex(i);
        }
    }

    private IEnumerator ScrollToTop()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.normalizedPosition = new Vector2(0, 1.0f);
    }

    private void HandleError(string message)
    {
        skeletonLoading.SetActive(false);
        _isBetsHistoryRefreshing = false;
        betHistoryErrorTMP.gameObject.SetActive(true);
        betHistoryErrorTMP.text = message;
        betsHistoryTotalInfo.SetData(0, 0, 0, 0, 0,
            0);
        Debug.LogError(message);
    }

    private void SetLoadingState(bool isLoading)
    {
        skeletonLoading.SetActive(isLoading);

        if (!isLoading)
        {
            betHistoryErrorTMP.gameObject.SetActive(false);
        }

        _isBetsHistoryRefreshing = isLoading;
    }
}