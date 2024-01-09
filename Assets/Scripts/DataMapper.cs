﻿using System;
using System.Collections;
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
    [SerializeField] private SwipeMenu swipeMenu;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject dataMapperSkeletonLoading;
    public static bool MatchesAvailable;
    private float _cooldownTimer;
    private readonly float _cooldownPeriod = 3f;

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
        InitializeUserData();
        if (!(_cooldownTimer <= 0)) return;
        
        _cooldownTimer = _cooldownPeriod;

        noMatchesPanel.SetActive(false);
        dataMapperSkeletonLoading.SetActive(true);
        matchPanelParent.ClearExistingElementsInParent();

        MatchesRepository.GetBettingAvailableMatches()
            .Then(matches =>
            {
                BetsRepository.GetAllBetsByUserId(UserData.UserId)
                    .Then(bets =>
                    {
                        BetCache.Bets = bets;
                    })
                    .Catch(exception => Debug.LogError(exception.Message))
                    .Finally(() =>
                    {
                        OnMapData?.Invoke();
                        CreateMatchViews(matches);
                    });
            }).Catch(exception =>
            {
                MatchesAvailable = false;
                Debug.LogError(exception.Message);
                dataMapperSkeletonLoading.SetActive(false);
            });
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

            MatchesAvailable = true;
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