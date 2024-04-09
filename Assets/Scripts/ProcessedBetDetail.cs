using System;
using Libs.Models;

public class ProcessedBetDetail
{
    public readonly Bet Bet;
    public readonly Match Match;
    public readonly DateTime DateTime;
    public readonly bool IsWinner;
    public readonly double MoneyLostOrGained;

    public ProcessedBetDetail(Bet bet, Match match, DateTime dateTime, bool isWinner, double moneyLostOrGained)
    {
        Bet = bet;
        Match = match;
        DateTime = dateTime;
        IsWinner = isWinner;
        MoneyLostOrGained = moneyLostOrGained;
    }
}