using System;
using System.Collections.Generic;
using Libs.Helpers;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.Serialization;

public class BetsController : MonoBehaviour
{
    [SerializeField] private BetsHandler betsHandler;
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private DataFetcher dataFetcher;
    private BetButtonEventArgs _betButtonEventArgs;

    public static event Action OnBetPosted;

    private void OnEnable()
    {
        betsHandler.OnBetSubmitted += HandleBetSubmitted;
        BetButtonView.OnButtonClick += InitializeBetFields;
    }

    private void HandleBetSubmitted(double betAmount)
    {
        _betButtonEventArgs.MatchViewParent.HideAllButtons();

        BetRequest newBetRequest = CreateNewBetRequest(betAmount);

        MatchesRepository.GetMatchById(newBetRequest.MatchId).Then(match =>
        {
            if (match.IsBettingAvailable)
            {
                BetsRepository.SaveBet(newBetRequest).Then(betId =>
                {
                    InfoPanelManager.ShowPanel(ColorHelper.LightGreen,
                        $"Bet has been successfully made. \nContestant name: {_betButtonEventArgs.Contestant.Name} \nBet amount: {betAmount}$ \nCoefficient: {_betButtonEventArgs.Contestant.Coefficient}");

                    UpdateUserBalance(betAmount);
                    UpdateCache(betId, newBetRequest);

                    OnBetPosted?.Invoke();
                }).Catch(exception =>
                {
                    InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                        $"Error to make bet. {exception.Message}");
                });
            }
            else
            {
                ShowErrorAndButtons(ColorHelper.HotPink,
                    "Betting not available for this match");
            }
        }).Catch(exception =>
        {
            ShowErrorAndButtons(ColorHelper.HotPink,
                $"Error to get match by id for bet. {exception.Message}");
        });
    }

    private void UpdateUserBalance(double betAmount)
    {
        UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
        {
            user.balance = moneyView.Balance - betAmount;

            UserRepository.UpdateUserInfo(user).Then(helper =>
            {
                moneyView.Balance -= betAmount;
                Debug.Log("success money update");
            }).Catch(exception =>
            {
                InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                    $"Error to update balance. {exception.Message}");
            });
        }).Catch(exception =>
        {
            InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                $"Error to get user by id. {exception.Message}");
        });
    }

    private BetRequest CreateNewBetRequest(double betAmount)
    {
        var newBetRequest = new BetRequest()
        {
            BetAmount = betAmount, ContestantId = _betButtonEventArgs.Contestant.Id, UserId = UserData.UserId,
            MatchId = _betButtonEventArgs.MatchId, IsActive = true
        };
        return newBetRequest;
    }

    private static void UpdateCache(string betId, BetRequest newBetRequest)
    {
        Bet newBet = new()
        {
            BetId = betId, MatchId = newBetRequest.MatchId, IsActive = newBetRequest.IsActive,
            BetAmount = newBetRequest.BetAmount, ContestantId = newBetRequest.ContestantId,
            UserId = newBetRequest.UserId
        };

        if (BetCache.Bets == null)
            BetCache.Bets = new List<Bet> { newBet };
        else
            BetCache.Bets.Add(newBet);

        ActiveBetsCache.AddActiveBetId(betId);
    }


    private void InitializeBetFields(object sender, BetButtonEventArgs args)
    {
        _betButtonEventArgs = args;
        betsHandler.InitializeBetMenu();
    }

    private void ShowErrorAndButtons(Color32 errorColor, string errorMessage)
    {
        InfoPanelManager.ShowPanel(errorColor, errorMessage);
        _betButtonEventArgs.MatchViewParent.ShowAllButtons();

        dataFetcher.FetchData();
    }
}