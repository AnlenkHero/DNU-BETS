using System;
using System.Collections;
using Libs.Models;
using Libs.Repositories;
using UnityEngine;

public class BetsController : MonoBehaviour
{
    [SerializeField] private BetsHandler betsHandler;
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private DataMapper dataMapper;
    private BetButtonEventArgs _betButtonEventArgs;

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

        var newBet = new Bet()
        {
            BetAmount = betAmount, ContestantId = _betButtonEventArgs.Contestant.Id, UserId = UserData.UserId,
            MatchId = _betButtonEventArgs.MatchId, IsActive = true
        };
        MatchesRepository.GetMatchById(newBet.MatchId).Then(match =>
        {
            if (match.IsBettingAvailable)
            {
                BetsRepository.SaveBet(newBet).Then(betId =>
                {
                    InfoPanel.ShowPanel(new Color32(0x2F, 0xFF, 0x2F, 0xFF),
                        $"Bet has been successfully made. \nContestant name: {_betButtonEventArgs.Contestant.Name} \nBet amount: {betAmount}$ \nCoefficient: {_betButtonEventArgs.Contestant.Coefficient}");
                    var tempBalance = moneyView.Balance - betAmount;
                    UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
                    {
                        user.Balance = tempBalance;

                        UserRepository.UpdateUserBalance(user).Then(helper =>
                        {
                            moneyView.Balance -= betAmount;
                            Debug.Log("success money update");
                        }).Catch(exception =>
                        {
                            InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                                $"Error to update balance. {exception.Message}");
                        });
                    }).Catch(exception =>
                    {
                        InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                            $"Error to get user by id. {exception.Message}");
                    });
                    ;
                }).Catch(exception =>
                {
                    InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                        $"Error to make bet. {exception.Message}");
                });
            }
            else
            {
                InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                    "Betting not available for this match");
                foreach (var betButtonView in _betButtonEventArgs.MatchViewParent.buttonViews)
                {
                    betButtonView.Show();
                }

                dataMapper.MapData();
            }
        }).Catch(exception =>
        {
            InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
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

    private IEnumerator WaitAndLog(BetButtonEventArgs args)
    {
        yield return new WaitForSeconds(2);
        Debug.Log(moneyView.Balance);
    }
}