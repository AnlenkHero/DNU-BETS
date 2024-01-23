using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Helpers;
using Libs.Repositories;
using RSG;
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
    private IEnumerable<Match> _availableMatches;
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 3f;
    private bool _isDataMappingRefreshing;
    public static event Action OnMapDataStarted;
    public static event Action OnMapDataFinished;

    private void OnEnable()
    {
        FirebaseGoogleLogin.OnLoginFinished += MapData;
        NetworkCheck.OnInternetEstablished += MapData;
        NameChanger.OnNameChanged += InitializeUserData;
        OnMapDataFinished += () => CreateMatchViews(_availableMatches.ToList());
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

        _cooldownTimer = _cooldownPeriod;
        _isDataMappingRefreshing = true;
        
        OnMapDataStarted?.Invoke();
        
        InitializeUserData();

        PrepareToRefresh();
        
        BetCache.Bets = null;
        MatchCache.Matches = null;

        var promises = new List<IPromise> { WrapGetMatches(), WrapGetAllBetsByUserId(UserData.UserId) };

        Promise.All(promises)
            .Catch(HandleDataMappingError)
            .Finally(() =>
            {
                OnMapDataFinished?.Invoke();
                _isDataMappingRefreshing = false;
            });
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


    private IPromise WrapGetMatches()
    {
        var promise = new Promise();

        MatchesRepository.GetAllMatches().Then(list =>
            {
                MatchCache.Matches = list;
                _availableMatches = list.Where(match => match.IsBettingAvailable);
                promise.Resolve();
            })
            .Catch(exception => promise.Reject(exception));

        return promise;
    }

    private IPromise WrapGetAllBetsByUserId(string userId)
    {
        var promise = new Promise();

        BetsRepository.GetAllBetsByUserId(userId)
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