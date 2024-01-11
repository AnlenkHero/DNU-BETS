using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
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
        DataMapper.OnMapData += RefreshBetsHistory;
        BetsController.OnBetPosted += RefreshBetsHistory;
    }

    private void RefreshBetsHistory()
    {
        if (_isBetsHistoryRefreshing) return;
        betHistoryParent.ClearExistingElementsInParent();
        FetchAndProcessBets();
    }


    private void FetchAndProcessBets()
    {
        SetLoadingState(true);
        MatchesRepository.GetAllMatches()
            .Then(matches =>
            {
                if (BetCache.Bets != null)
                    ProcessBets(BetCache.Bets, matches);
                else
                {
                    SetLoadingState(false);
                    HandleError("No bets found for user");
                }
            })
            .Catch(exception =>
            {
                SetLoadingState(false);
                HandleError($"Failed to load matches {exception}");
            });
    }


    private void ProcessBets(List<Bet> bets, List<Match> matches)
    {
        var betHistoryElements = new List<BetHistoryElement>();
        BetsProcessor.ProcessBets(bets, matches, stats =>
        {
            foreach (var processedBet in stats.ProcessedBets)
            {
                var tempBetHistoryElement = Instantiate(betHistoryElement, betHistoryParent);

                tempBetHistoryElement.SetData(
                    processedBet.Match.MatchTitle,
                    processedBet.Match.Contestants.Find(c => c.Id == processedBet.Bet.ContestantId)?.Name,
                    processedBet.Match.Contestants.Find(c => c.Id == processedBet.Bet.ContestantId)?.Coefficient ?? 0,
                    processedBet.Bet.BetAmount,
                    processedBet.Bet.IsActive,
                    processedBet.IsWinner,
                    processedBet.DateTime,
                    processedBet.Match.IsMatchCanceled
                );

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