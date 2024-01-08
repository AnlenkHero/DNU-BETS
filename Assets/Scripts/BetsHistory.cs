using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    [SerializeField] private Button refreshButton;
    [SerializeField] private TextMeshProUGUI betHistoryErrorTMP;
    private bool _isBetsHistoryRefreshing;


    private void OnEnable()
    {
        DataMapper.OnMapData += RefreshBetsHistory;
        BetsController.OnBetPosted += RefreshBetsHistory;
    }

    private void Start()
    {
        refreshButton.onClick.AddListener(RefreshBetsHistory);
    }

    private void RefreshBetsHistory()
    {
        if (_isBetsHistoryRefreshing) return;
        ClearExistingBetsHistory();
        FetchAndProcessBets();
    }


    private void FetchAndProcessBets()
    {
        SetLoadingState(true);
        MatchesRepository.GetAllMatches()
            .Then(matches =>
            {
                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets =>  ProcessBets(bets, matches))
                    .Catch(exception =>
                    {
                        SetLoadingState(false);
                        HandleError($"No bets found for user {exception}");
                    });
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
        var betsWon = 0;
        var betsLost = 0;
        double moneyGained = 0;
        double moneyLost = 0;

        foreach (var bet in bets)
        {
            Match match = matches.FirstOrDefault(x => x.Id == bet.MatchId);

            if (match == null) continue;

            var contestant = match.Contestants.FirstOrDefault(c => c.Id == bet.ContestantId);
            var dateTime = DateTime.TryParseExact(match.FinishedDateUtc, "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue)
                ? dateValue
                : DateTime.Now;

            var tempBetHistoryElement = Instantiate(betHistoryElement, betHistoryParent);

            tempBetHistoryElement.SetData(match.MatchTitle, contestant.Name, contestant.Coefficient, bet.BetAmount,
                bet.IsActive, contestant.Winner, dateTime, match.IsMatchCanceled);

            betHistoryElements.Add(tempBetHistoryElement);

            if (contestant.Winner)
            {
                betsWon++;
                moneyGained += contestant.Coefficient * bet.BetAmount;
            }
            else if (!bet.IsActive && !match.IsMatchCanceled)
            {
                moneyLost -= bet.BetAmount;
                betsLost++;
            }
        }

        betHistoryElements.Sort((a, b) => b.Date.CompareTo(a.Date));
        for (var i = 0; i < betHistoryElements.Count; i++)
        {
            betHistoryElements[i].transform.SetSiblingIndex(i);
        }


        betsHistoryTotalInfo.SetData(bets.Count, betsWon, betsLost, moneyGained, moneyLost);

        SetLoadingState(false);
        StartCoroutine(ScrollToTop());
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
        refreshButton.gameObject.SetActive(true);
        Debug.LogError(message);
    }

    private void SetLoadingState(bool isLoading)
    {
        skeletonLoading.SetActive(isLoading);

        if (!isLoading)
        {
            betHistoryErrorTMP.gameObject.SetActive(false);
            refreshButton.gameObject.SetActive(false);
        }

        _isBetsHistoryRefreshing = isLoading;
    }

    private void ClearExistingBetsHistory()
    {
        foreach (Transform child in betHistoryParent)
        {
            Destroy(child.gameObject);
        }
    }
}