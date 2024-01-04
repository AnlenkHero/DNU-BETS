using System;
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

        var newBetRequest = new BetRequest()
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
                    InfoPanel.ShowPanel(ColorHelper.LightGreen,
                        $"Bet has been successfully made. \nContestant name: {_betButtonEventArgs.Contestant.Name} \nBet amount: {betAmount}$ \nCoefficient: {_betButtonEventArgs.Contestant.Coefficient}");

                    var tempBalance = moneyView.Balance - betAmount;

                    UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
                    {
                        user.balance = tempBalance;

                        UserRepository.UpdateUserInfo(user).Then(helper =>
                        {
                            moneyView.Balance -= betAmount;
                            Debug.Log("success money update");
                        }).Catch(exception =>
                        {
                            InfoPanel.ShowPanel(ColorHelper.HotPink,
                                $"Error to update balance. {exception.Message}");
                        });
                    }).Catch(exception =>
                    {
                        InfoPanel.ShowPanel(ColorHelper.HotPink,
                            $"Error to get user by id. {exception.Message}");
                    });

                    Bet newBet = new()
                    {
                        BetId = betId, MatchId = newBetRequest.MatchId, IsActive = newBetRequest.IsActive,
                        BetAmount = newBetRequest.BetAmount, ContestantId = newBetRequest.ContestantId,
                        UserId = newBetRequest.UserId
                    };
                    
                    BetCache.Bets.Add(newBet);

                    OnBetPosted?.Invoke();
                }).Catch(exception =>
                {
                    InfoPanel.ShowPanel(ColorHelper.HotPink,
                        $"Error to make bet. {exception.Message}");
                });
            }
            else
            {
                InfoPanel.ShowPanel(ColorHelper.HotPink,
                    "Betting not available for this match");
                foreach (var betButtonView in _betButtonEventArgs.MatchViewParent.buttonViews)
                {
                    betButtonView.Show();
                }

                dataMapper.MapData();
            }
        }).Catch(exception =>
        {
            InfoPanel.ShowPanel(ColorHelper.HotPink,
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