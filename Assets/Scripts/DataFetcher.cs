using System;
using System.Collections.Generic;
using Libs.Repositories;
using RSG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataFetcher : MonoBehaviour
{
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button refreshButton;
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 3f;
    private bool _isDataFetcherRefreshing;
    public static event Action OnFetchDataStarted;
    public static event Action OnFetchDataFinished;

    private void OnEnable()
    {
        FirebaseGoogleLogin.OnLoginFinished += FetchData;
        NetworkCheck.OnInternetEstablished += FetchData;
        NameChanger.OnNameChanged += InitializeUserData;
    }

    private void Awake()
    {
        refreshButton.onClick.AddListener(FetchData);
    }

    private void Update()
    {
        TimerCountdown();
    }

    private void TimerCountdown()
    {
        if (_cooldownTimer > 0)
            _cooldownTimer -= Time.deltaTime;
    }

    public void FetchData()
    {
        if (!(_cooldownTimer <= 0) || _isDataFetcherRefreshing) return;

        PrepareToRefresh();
        OnFetchDataStarted?.Invoke();
        InitializeUserData();
        HandleAllPromises();
    }

    private void HandleAllPromises()
    {
        var promises = new List<IPromise> { WrapGetMatches(), WrapGetAllBetsByUserId() };

        Promise.All(promises)
            .Catch(HandleDataFetchingError)
            .Finally(FinishFetching);
    }

    private void FinishFetching()
    {
        OnFetchDataFinished?.Invoke();
        _isDataFetcherRefreshing = false;
    }

    private void PrepareToRefresh()
    {
        _cooldownTimer = _cooldownPeriod;
        _isDataFetcherRefreshing = true;
        BetCache.Bets = null;
        MatchCache.Matches = null;
    }

    private void HandleDataFetchingError(Exception exception)
    {
        Debug.LogError(exception.Message);
        _isDataFetcherRefreshing = false;
    }

    private IPromise WrapGetMatches()
    {
        var promise = new Promise();

        MatchesRepository.GetAllMatches().Then(list =>
            {
                MatchCache.Matches = list;
                promise.Resolve();
            })
            .Catch(exception => promise.Reject(exception));

        return promise;
    }

    private IPromise WrapGetAllBetsByUserId()
    {
        var promise = new Promise();

        BetsRepository.GetAllBetsByUserId(UserData.UserId)
            .Then(list =>
            {
                BetCache.Bets = list;
                promise.Resolve();
            })
            .Catch(exception => promise.Reject(exception));

        return promise;
    }

    private void InitializeUserData()
    {
        nameText.text = UserData.Name;
        UserRepository.GetUserBalanceById(UserData.UserId)
            .Then(balance => { moneyView.Balance = balance; })
            .Catch(Debug.LogError);
    }
}