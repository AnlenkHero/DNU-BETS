using System;
using System.Collections;
using System.Collections.Generic;
using Libs.Models;
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
    [SerializeField] private SwipeMenu swipeMenu;
    [SerializeField] private Button refreshButton;
    public static bool MatchesAvailable;
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 3f;
    
    public static event Action OnMapData;

    private void OnEnable()
    {
        FirebaseGoogleLogin.OnLoginFinished += MapData;
        FirebaseGoogleLogin.OnLoginFinished += () => StartCoroutine(InitializeUserData());
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
        if (!(_cooldownTimer <= 0)) return;
        
        _cooldownTimer = _cooldownPeriod;
        
        BetCache.Bets = new List<Bet>();
        
        ClearExistingMatches();
        
        MatchesRepository.GetBettingAvailableMatches()
            .Then(matches =>
            {
                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets => BetCache.Bets = bets)
                    .Catch(exception => Debug.LogError(exception.Message))
                    .Finally(() => CreateMatchViews(matches));
            }).Catch(exception =>
            {
                MatchesAvailable = false;
                Debug.LogError(exception.Message);
            });
        
        OnMapData?.Invoke();
    }
    
    private void CreateMatchViews(List<Match> matches)
    {
        if (matches.Count == 0)
            Instantiate(noMatchesPanel,matchPanelParent);
        else
        {
            foreach (var match in matches)
            {
                var matchView = Instantiate(matchPanel, matchPanelParent);

                matchView.SetData(match);
            }

            MatchesAvailable = true;
        }
    }

    private void ClearExistingMatches()
    {
        foreach (Transform child in matchPanelParent)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator InitializeUserData()
    {
        while (true)
        {
            nameText.text = UserData.Name;
            UserRepository.GetUserBalanceById(UserData.UserId)
                .Then(balance => { moneyView.Balance=balance; })
                .Catch(Debug.LogError); 
            yield return new WaitForSeconds(10f);
        }
    }

}