using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Models;
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
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 10f;

    private void OnEnable()
    {
        DataMapper.OnMapData += InitializeBetsHistory;
        BetsController.OnBetPosted += InitializeBetsHistory;
    }

    private void Start()
    {
        InitializeBetsHistory();
    }
    
    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    private void InitializeBetsHistory()
    {
        if (!_isBetsHistoryRefreshing && _cooldownTimer <= 0)
        {
            _cooldownTimer = _cooldownPeriod;
            StartCoroutine(InitializeBetsHistoryCoroutine());
        }
    }

    private IEnumerator InitializeBetsHistoryCoroutine()
    {
        _isBetsHistoryRefreshing = true;
        yield return StartCoroutine(ClearExistingBetsHistory());

        Dictionary<string, Match> matchesDictionary = new Dictionary<string, Match>();

        bool isTimedOut = false;
        StartCoroutine(TimeoutCoroutine(() =>
        {
            isTimedOut = true;
            _isBetsHistoryRefreshing = false;
        }));

        MatchesRepository.GetAllMatches()
            .Then(matches =>
            {
                if (isTimedOut)
                {
                    _isBetsHistoryRefreshing = false;
                    return;
                }

                foreach (var match in matches)
                {
                    matchesDictionary[match.Id] = match;
                }

                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets =>
                    {
                        if (isTimedOut)
                        {
                            _isBetsHistoryRefreshing = false;
                            return;
                        }

                        ProcessBets(bets, matchesDictionary);
                    })
                    .Catch(exception =>
                    {
                        _isBetsHistoryRefreshing = false;
                        Debug.LogError(exception.Message);
                    })
                    .Finally(() =>
                    {
                        _isBetsHistoryRefreshing = false;
                        scrollRect.velocity = new Vector2(0, -float.MaxValue);
                    });
            })
            .Catch(exception =>
            {
                _isBetsHistoryRefreshing = false;
                Debug.LogError($"Failed to load matches: {exception.Message}");
            })
            .Finally(() => _isBetsHistoryRefreshing = false);
    }

    private void ProcessBets(List<Bet> bets, Dictionary<string, Match> matchesDictionary)
    {
        var betHistoryElements = new List<BetHistoryElement>();
        var betsWon = 0;
        var betsLost = 0;
        double moneyGained = 0;
        double moneyLost = 0;

        foreach (var bet in bets)
        {
            if (!matchesDictionary.TryGetValue(bet.MatchId, out var match))
            {
                Debug.LogError($"Match with ID {bet.MatchId} not found.");
                continue;
            }

            var contestant = match.Contestants.FirstOrDefault(c => c.Id == bet.ContestantId);
            var tempBetHistoryElement = Instantiate(betHistoryElement, betHistoryParent);
            var dateTime = DateTime.TryParseExact(match.FinishedDateUtc, "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue)
                ? dateValue
                : DateTime.Now;

            tempBetHistoryElement.SetData(match.MatchTitle, contestant.Name, contestant.Coefficient, bet.BetAmount,
                bet.IsActive, contestant.Winner, dateTime, match.IsMatchCanceled);

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
        }

        betHistoryElements.Sort((a, b) => b.Date.CompareTo(a.Date));
        foreach (var element in betHistoryElements)
        {
            element.transform.SetSiblingIndex(betHistoryParent.childCount);
        }

        betsHistoryTotalInfo.SetData(bets.Count, betsWon, betsLost, moneyGained, moneyLost);
        _isBetsHistoryRefreshing = false;
    }

    private IEnumerator ClearExistingBetsHistory()
    {
        foreach (Transform child in betHistoryParent)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator TimeoutCoroutine(Action onTimeout, float timeoutSeconds = 30f)
    {
        yield return new WaitForSeconds(timeoutSeconds);
        onTimeout?.Invoke();
    }
}