using System;
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
    private BetButtonEventArgs _betButtonEventArgs;

    public static event Action OnBetPosted;

    private void OnEnable()
    {
        betsHandler.OnBetSubmit += HandleBetSubmit;
        BetButtonView.OnButtonClick += InitializeBetFields;
    }

    private void OnDisable()
    {
        betsHandler.OnBetSubmit -= HandleBetSubmit;
        BetButtonView.OnButtonClick -= InitializeBetFields;
    }

    private void HandleBetSubmit(double betAmount)
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
        
        BetsRepository.SaveBet(newBetRequest).Then(betId =>
        {
            InfoPanelManager.ShowPanel(ColorHelper.LightGreen,
                $"Bet has been successfully made. \nContestant name: {_betButtonEventArgs.Contestant.Name} \nBet amount: {betAmount}$ \nCoefficient: {_betButtonEventArgs.Contestant.Coefficient}");

            moneyView.Balance -= betAmount;

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
                $"Error making a bet. {exception.Message}");
        });
    }


    private void InitializeBetFields(object sender, BetButtonEventArgs args)
    {
        _betButtonEventArgs = args;
        betsHandler.InitializeBetMenu();
    }
}