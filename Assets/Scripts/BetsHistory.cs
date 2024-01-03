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
    [SerializeField] private GameObject waitingObject;
    [SerializeField] private TextMeshProUGUI betHistoryErrorTMP;
    private bool _isBetsHistoryRefreshing;
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 3f;

    private void OnEnable()
    {
        DataMapper.OnMapData += RefreshBetsHistory;
        BetsController.OnBetPosted += RefreshBetsHistory;
    }

    private void Start()
    {
        refreshButton.onClick.AddListener(RefreshBetsHistory);
        RefreshBetsHistory();
    }
    
    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    private void RefreshBetsHistory()
    {
        if (!_isBetsHistoryRefreshing && _cooldownTimer <= 0)
        {
            _cooldownTimer = _cooldownPeriod;
            StartCoroutine(RefreshBetsHistoryCoroutine());
        }
    }

    private IEnumerator RefreshBetsHistoryCoroutine()
    {
        SetLoadingState(true);
        
        yield return StartCoroutine(ClearExistingBetsHistory());

        bool isTimedOut = false;
        StartCoroutine(TimeoutCoroutine(() =>
        {
            isTimedOut = true;
            _isBetsHistoryRefreshing = false;
        }));
        
        yield return FetchAndProcessBets(isTimedOut);
        SetLoadingState(false);
    }

    private IEnumerator FetchAndProcessBets(bool isTimedOut)
    {
        MatchesRepository.GetAllMatches()
            .Then(matches =>
            {
                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets => ProcessBets(bets, matches))
                    .Catch(exception => HandleError(exception.Message));
            })
            .Catch(exception => HandleError(exception.Message));

        if (isTimedOut)
        {
            HandleError("Timed out");
            yield break;
        }


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
            else if (!bet.IsActive)
            {
                moneyLost -= bet.BetAmount;
                betsLost++;
            }
        }


        StartCoroutine(WaitForElementsAndResetScrollRoutine(betHistoryElements));
        betsHistoryTotalInfo.SetData(bets.Count, betsWon, betsLost, moneyGained, moneyLost);
        _isBetsHistoryRefreshing = false;
    }
    
    private IEnumerator WaitForElementsAndResetScrollRoutine(List<BetHistoryElement> elements)
    {
        yield return StartCoroutine(SortElementCoroutine(elements));
        scrollRect.normalizedPosition = new Vector2(0, 1.0f);
    }

    private IEnumerator SortElementCoroutine(List<BetHistoryElement> elements)
    {
        elements.Sort((a, b) => b.Date.CompareTo(a.Date));
        foreach (var element in elements)
        {
            element.transform.SetSiblingIndex(betHistoryParent.childCount);
        }
        yield return null;
    }
    private void HandleError(string message)
    {
        _isBetsHistoryRefreshing = false;
        betHistoryErrorTMP.gameObject.SetActive(true);
        betHistoryErrorTMP.text = message;
        refreshButton.gameObject.SetActive(true);
        Debug.LogError(message);
    }
    
    private void SetLoadingState(bool isLoading)
    {
        skeletonLoading.SetActive(isLoading);
        waitingObject.SetActive(isLoading);
        
        if (!isLoading)
        {
            betHistoryErrorTMP.gameObject.SetActive(false);
            refreshButton.gameObject.SetActive(false);
        }

        _isBetsHistoryRefreshing = isLoading;
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