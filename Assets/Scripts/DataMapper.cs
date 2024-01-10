using System;
using System.Collections.Generic;
using Libs.Helpers;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Match = Libs.Models.Match;

public class DataMapper : MonoBehaviour
{
    [SerializeField] private Transform matchPanelParent;
    [SerializeField] private MatchView matchPanel;
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject noMatchesPanel;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject dataMapperSkeletonLoading;
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 3f;
    private bool _isDataMappingRefreshing;
    public static event Action OnMapData;

    private void OnEnable()
    {
        FirebaseGoogleLogin.OnLoginFinished += MapData;
        NetworkCheck.OnInternetEstablished += MapData;
    }

    private void Awake()
    {
        refreshButton.onClick.AddListener(MapData);
    }

    private void Update()
    {
        if (_cooldownTimer > 0)
            _cooldownTimer -= Time.deltaTime;
    }

    public void MapData()
    {
        if (!(_cooldownTimer <= 0) || _isDataMappingRefreshing) return;

        InitializeUserData();
        
        _cooldownTimer = _cooldownPeriod;
        _isDataMappingRefreshing = true;

        PrepareToRefresh();

        MatchesRepository.GetBettingAvailableMatches()
            .Then(matches =>
            {
                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets => BetCache.Bets = bets)
                    .Catch(exception => Debug.LogError(exception.Message))
                    .Finally(() =>
                    {
                        OnMapData?.Invoke();
                        CreateMatchViews(matches);
                        _isDataMappingRefreshing = false;
                    });
            }).Catch(HandleDataMappingError);
    }

    private void PrepareToRefresh()
    {
        noMatchesPanel.SetActive(false);
        dataMapperSkeletonLoading.SetActive(true);
        matchPanelParent.ClearExistingElementsInParent();
    }

    private void HandleDataMappingError(Exception exception)
    {
        Debug.LogError(exception.Message);
        dataMapperSkeletonLoading.SetActive(false);
        _isDataMappingRefreshing = false;
    }

    private void CreateMatchViews(List<Match> matches)
    {
        if (matches.Count == 0)
            noMatchesPanel.SetActive(true);
        else
        {
            foreach (var match in matches)
            {
                var matchView = Instantiate(matchPanel, matchPanelParent);

                matchView.SetData(match);
            }
        }

        dataMapperSkeletonLoading.SetActive(false);
    }

    private void InitializeUserData()
    {
        nameText.text = UserData.Name;
        UserRepository.GetUserBalanceById(UserData.UserId)
            .Then(balance => { moneyView.Balance = balance; })
            .Catch(Debug.LogError);
    }
}