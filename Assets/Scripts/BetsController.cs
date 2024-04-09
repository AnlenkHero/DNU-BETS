﻿using System;
using System.Collections.Generic;
using Libs.Helpers;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using UnityEngine;

public class BetsController : MonoBehaviour
{
    [SerializeField] private BetsHandler betsHandler;
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private DataMapper dataMapper;
    private BetButtonEventArgs _betButtonEventArgs;

    public static event Action OnBetPosted;

    private void OnEnable()
    {
        betsHandler.OnBetSubmitted += HandleBetSubmitted;
        BetButtonView.OnButtonClick += InitializeBetFields;
    }

    private void OnDisable()
    {
        betsHandler.OnBetSubmitted -= HandleBetSubmitted;
        BetButtonView.OnButtonClick -= InitializeBetFields;
    }

    private void HandleBetSubmitted(double betAmount)
    {
        foreach (var betButtonView in _betButtonEventArgs.MatchViewParent.buttonViews)
        {
            betButtonView.Hide();
        }

        var newBetRequest = new BetRequest
        {
            BetAmount = betAmount, ContestantId = _betButtonEventArgs.Contestant.Id, UserId = UserData.UserId,
            MatchId = _betButtonEventArgs.MatchId, IsActive = true
        };
        MatchesRepository.GetMatchById(newBetRequest.MatchId).Then(match =>
        {
            if (match.IsBettingAvailable)
            {
                BetsRepository.SaveBet(newBetRequest).Then(betId =>
                {
                    InfoPanelManager.ShowPanel(ColorHelper.LightGreen,
                        $"Bet has been successfully made. \nContestant name: {_betButtonEventArgs.Contestant.Name} \nBet amount: {betAmount}$ \nCoefficient: {_betButtonEventArgs.Contestant.Coefficient}");

                    moneyView.Balance =- betAmount;

                    Bet newBet = new()
                    {
                        BetId = betId, MatchId = newBetRequest.MatchId, IsActive = newBetRequest.IsActive,
                        BetAmount = newBetRequest.BetAmount, ContestantId = newBetRequest.ContestantId,
                        UserId = newBetRequest.UserId
                    };

                    if (BetCache.Bets == null)
                    {
                        BetCache.Bets = new List<Bet> { newBet };
                    }
                    else
                    {
                        BetCache.Bets.Add(newBet);
                    }
                    
                    ActiveBetsCache.AddActiveBetId(betId);
                    
                    OnBetPosted?.Invoke();
                }).Catch(exception =>
                {
                    InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                        $"Error to make bet. {exception.Message}");
                });
            }
            else
            {
                InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                    "Betting not available for this match");
                foreach (var betButtonView in _betButtonEventArgs.MatchViewParent.buttonViews)
                {
                    betButtonView.Show();
                }

                dataMapper.MapData();
            }
        }).Catch(exception =>
        {
            InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                $"Error to get match by id for bet. {exception.Message}");
            foreach (var betButtonView in _betButtonEventArgs.MatchViewParent.buttonViews)
            {
                betButtonView.Show();
            }

            dataMapper.MapData();
        });
    }


    private void InitializeBetFields(object sender, BetButtonEventArgs args)
    {
        _betButtonEventArgs = args;
        betsHandler.InitializeBetMenu();
    }
}