using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class BetsHistory : MonoBehaviour
{
    [SerializeField] private BetHistoryElement betHistoryElement;
    [SerializeField] private Transform betHistoryParent;
    [SerializeField] private BetsHistoryTotalInfo betsHistoryTotalInfo;
    [SerializeField] private ScrollRect scrollRect;
    private bool _isBetsHistoryRefreshing;


    private void OnEnable()
    {
        DataMapper.OnMapData += InitializeBetsHistory;
        BetsController.OnBetPosted += InitializeBetsHistory;
    }

    private void OnDisable()
    {
        DataMapper.OnMapData -= InitializeBetsHistory;
        BetsController.OnBetPosted -= InitializeBetsHistory;
    }

    private void Start()
    {
        InitializeBetsHistory();
    }

    private void InitializeBetsHistory()
    {
        if (!_isBetsHistoryRefreshing)
            StartCoroutine(InitializeBetsHistoryCoroutine());
    }

    private IEnumerator InitializeBetsHistoryCoroutine()
    {
        _isBetsHistoryRefreshing = true;
        yield return StartCoroutine(ClearExistingBetsHistory());
        BetsRepository.GetAllBetsByUserId(UserData.UserId)
            .Then(bets =>
            {
                var betHistoryElements = new List<BetHistoryElement>();
                var betsToProcess = bets.Count;
                var betsWon = 0;
                var betsLost = 0;
                double moneyGained = 0;
                double moneyLost = 0;

                foreach (var bet in bets)
                {
                    MatchesRepository.GetMatchById(bet.MatchId).Then(match =>
                    {
                        var contestant =
                            match.Contestants.FirstOrDefault(contestant => contestant.Id == bet.ContestantId);
                        var tempBetHistoryElement = Instantiate(betHistoryElement, betHistoryParent);
                        var dateTime = DateTime.TryParseExact(match.FinishedDateUtc, "MM/dd/yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue)
                            ? dateValue
                            : DateTime.Now;

                        tempBetHistoryElement.SetData(match.MatchTitle, contestant.Name, contestant.Coefficient,
                            bet.BetAmount, bet.IsActive, contestant.Winner, dateTime, match.IsMatchCanceled);

                        betHistoryElements.Add(tempBetHistoryElement);

                        if (contestant.Winner)
                        {
                            betsWon++;
                            moneyGained += contestant.Coefficient * bet.BetAmount;
                        }
                        else if (!bet.IsActive)
                        {
                            moneyLost -= bet.BetAmount;
                            betsLost++;
                        }

                        betsToProcess--;
                        if (betsToProcess != 0) return;
                        betHistoryElements.Sort((a, b) => b.Date.CompareTo(a.Date));
                        for (var i = 0; i < betHistoryElements.Count; i++)
                        {
                            betHistoryElements[i].transform.SetSiblingIndex(i);
                        }

                        betsHistoryTotalInfo.SetData(bets.Count, betsWon, betsLost, moneyGained, moneyLost);
                        scrollRect.normalizedPosition = new Vector2(0, 1.5f);
                        _isBetsHistoryRefreshing = false;
                    });
                }
            })
            .Catch(exception =>
            {
                Debug.LogError(exception.Message);
                _isBetsHistoryRefreshing = false;
            });
    }

    private IEnumerator ClearExistingBetsHistory()
    {
        foreach (Transform child in betHistoryParent)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitForSeconds(0.5f);
    }
}