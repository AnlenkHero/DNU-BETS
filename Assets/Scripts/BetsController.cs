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
        var newBet = new Bet(){BetAmount = betAmount, ContestantId = _betButtonEventArgs.Contestant.Id, UserId = UserData.UserId, MatchId = _betButtonEventArgs.MatchId};
        MatchesRepository.GetMatchById(newBet.MatchId).Then(match =>
        {
            if (match.IsBettingAvailable)
            {
                BetsRepository.SaveBet(newBet).Then(_ =>
                {
                    foreach (var betButtonView in _betButtonEventArgs.MatchViewParent.buttonViews)
                    {
                        betButtonView.Hide();
                    }
                    moneyView.SubtractMoney(betAmount);
                }).Catch(exception => { Debug.Log($"Error to make bet {exception.Message}"); });
            }
            else
            {
                Debug.Log("Betting no available for this match");
                dataMapper.MapData();
            }
        }).Catch((exception =>
        {
            Debug.Log($"Error get match by id for bet {exception.Message}");
            dataMapper.MapData();
        } ));
        //  Debug.Log(moneyView.Balance);
        // StartCoroutine(WaitAndLog(_betButtonEventArgs));
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