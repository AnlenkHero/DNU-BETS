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
    [SerializeField] private Button updateMatchesButton;
    public static bool MatchesAvailable;

    private void Awake()
    {
        updateMatchesButton.onClick.AddListener(MapData);
    }

    private void OnEnable()
    {
        FirebaseGoogleLogin.OnLoginFinished += MapData;
        FirebaseGoogleLogin.OnLoginFinished += () => StartCoroutine(InitializeUserData());
        NetworkCheck.OnInternetEstablished += MapData;
    }
    
    public void MapData()
    {
        StartCoroutine(MapDataCoroutine());
    }

    private IEnumerator MapDataOverTime()
    {
        while (true)
        {
            StartCoroutine(MapDataCoroutine());
            yield return new WaitForSeconds(100f);
        }
    }

    private IEnumerator MapDataCoroutine()
    {
        BetCache.Bets = new List<Bet>();
        yield return StartCoroutine(ClearExistingMatches());
        MatchesRepository.GetBettingAvailableMatches()
            .Then(matches =>
            {
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
        if(matches.Count == 0)
            noMatchesPanel.SetActive(true);
        else
        {
            foreach (var match in matches)
            {
                var matchView = Instantiate(matchPanel, matchPanelParent);

                matchView.SetData(match);
            }
            MatchesAvailable = true;
            noMatchesPanel.SetActive(false);
        }
    }

    private IEnumerator ClearExistingMatches()
    {
        foreach (Transform child in matchPanelParent)
        {
            Destroy(child.gameObject);
        }
        
        yield return new WaitForEndOfFrame();
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