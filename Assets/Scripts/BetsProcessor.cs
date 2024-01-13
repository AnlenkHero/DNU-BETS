using System;
using System.Collections.Generic;
using System.Globalization;
using Libs.Models;

public class BetsProcessor
{
    public struct ProcessedBetDetail
    {
        public readonly Bet Bet;
        public readonly Match Match;
        public readonly DateTime DateTime;
        public readonly bool IsWinner;

        public ProcessedBetDetail(Bet bet, Match match, DateTime dateTime, bool isWinner)
        {
            Bet = bet;
            Match = match;
            DateTime = dateTime;
            IsWinner = isWinner;
        }
    }
    
    public struct BetStats
    {
        public int BetsWon;
        public int BetsLost;
        public int MatchesCanceled;
        public double MoneyGained;
        public double MoneyLost;
        public List<ProcessedBetDetail> ProcessedBets;
    }

    public static void ProcessBets(List<Bet> bets, List<Match> matches, Action<BetStats> onProcessed)
    {
        var stats = new BetStats
        {
            BetsWon = 0,
            BetsLost = 0,
            MoneyGained = 0,
            MoneyLost = 0,
            ProcessedBets = new List<ProcessedBetDetail>()
        };

        foreach (var bet in bets)
        {
            Match match = matches.Find(x => x.Id == bet.MatchId);
            if (match == null)
            {
                stats.MatchesCanceled++;
                continue;
            }

            var contestant = match.Contestants.Find(c => c.Id == bet.ContestantId);
            DateTime dateTime = DateTime.TryParseExact(match.FinishedDateUtc, "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue)
                ? dateValue
                : DateTime.Now;

            stats.ProcessedBets.Add(new ProcessedBetDetail(bet,match,dateTime, contestant is { Winner: true }));

            if (match.IsMatchCanceled)
            {
                stats.MatchesCanceled++;
            }
            else if (contestant != null && contestant.Winner)
            {
                stats.BetsWon++;
                stats.MoneyGained += contestant.Coefficient * bet.BetAmount - bet.BetAmount;
            }
            else if (!bet.IsActive && !match.IsMatchCanceled)
            {
                stats.BetsLost++;
                stats.MoneyLost -= bet.BetAmount;
            }
        }

        onProcessed?.Invoke(stats);
    }
    
}
