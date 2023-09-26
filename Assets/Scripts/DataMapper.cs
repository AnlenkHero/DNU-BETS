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
    [SerializeField] private SwipeMenu swipeMenu;
    [SerializeField] private Button updateMatchesButton;
    public static bool MatchesAvailable;

    private void Awake()
    {
        updateMatchesButton.onClick.AddListener(MapData);
    }

    private void OnEnable()
    {
        FirebaseGoogleLogin.OnLoginFinished += MapData;
        NetworkCheck.OnInternetEstablished += MapData;
    }

    private void OnDisable()
    {
        NetworkCheck.OnInternetEstablished -= MapData;
        FirebaseGoogleLogin.OnLoginFinished -= MapData;
    }

    private void MapData()
    {
        StartCoroutine(MapDataCoroutine());
    }

    private IEnumerator MapDataCoroutine()
    {
        BetCache.Bets = new List<Bet>();
        yield return StartCoroutine(ClearExistingMatches());
        MatchesRepository.GetBettingAvailableMatches()
            .Then(matches =>
            {
                InitializeUserData();

                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets => BetCache.Bets = bets)
                    .Catch(exception => Debug.LogError(exception.Message))
                    .Finally(() => StartCoroutine(CreateMatchViews(matches)));
            }).Catch(exception =>
            {
                MatchesAvailable = false;
                Debug.LogError(exception.Message);
            });
    }


    private IEnumerator CreateMatchViews(List<Match> matches)
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ClearExistingMatches());
        foreach (var match in matches)
        {
            var matchView = Instantiate(matchPanel, matchPanelParent);

            matchView.SetData(match);
        }
        MatchesAvailable = true;
    }

    private IEnumerator ClearExistingMatches()
    {
        foreach (Transform child in matchPanelParent)
        {
            Destroy(child.gameObject);
        }
        
        yield return new WaitForEndOfFrame();
    }

    private void InitializeUserData()
    {
        nameText.text = UserData.Name;
        UserRepository.GetUserBalanceById(UserData.UserId).Then(balance => { moneyView.SetMoney(balance); });
    }
}