using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Image profileImageBorder;
    [SerializeField] private Image profileImageOuterBorder;
    [SerializeField] private Button getGamblerInfo;

    public void SetData(User user, Color color, List<Match> matches, [CanBeNull] Gradient wobbleGradient = null,
        [CanBeNull] Material material = null)
    {
        nameText.text = user.userName;
        moneyText.text =
            $"{user.balance.ToString(CultureInfo.InvariantCulture)}<color={ColorHelper.LightGreenString}>$</color>";
        profileImageBorder.color = color;
        profileImageOuterBorder.color = color;
        nameText.color = color;
        moneyText.color = color;

        getGamblerInfo.onClick.AddListener(() => GetGamblerStats(user, matches));

        if (wobbleGradient != null)
            moneyText.gameObject.AddComponent<WordWobble>().rainbow = wobbleGradient;

        if (material != null)
        {
            profileImageOuterBorder.material = new Material(material);
            profileImageOuterBorder.gameObject.AddComponent<SpinAnimation>();
        }

        TextureLoader.LoadTexture(this, user.imageUrl, texture2D =>
        {
            if (texture2D != null)
                profileImage.texture = texture2D;
            else
                Debug.Log("Texture failed to load.");
        });
    }

    private void GetGamblerStats(User user, List<Match> matches)
    {
        if (matches.Count > 0)
        {
            BetsRepository.GetAllBetsByUserId(user.userId)
                .Then(bets => ProcessBets(user, bets, matches))
                .Catch(exception => Debug.LogError(exception.Message));
        }
    }

    private void ProcessBets(User user, List<Bet> bets, List<Match> matches)
    {
        var betsWon = 0;
        var betsLost = 0;
        var matchesCanceled = 0;
        double moneyGained = 0;
        double moneyLost = 0;

        foreach (var bet in bets)
        {
            Match match = matches.FirstOrDefault(x => x.Id == bet.MatchId);

            if (match == null) continue;

            var contestant = match.Contestants.FirstOrDefault(c => c.Id == bet.ContestantId);

            if (match.IsMatchCanceled)
            {
                matchesCanceled++;
            }

            if (contestant.Winner)
            {
                betsWon++;
                moneyGained += contestant.Coefficient * bet.BetAmount;
            }
            else if (!bet.IsActive && !match.IsMatchCanceled)
            {
                moneyLost -= bet.BetAmount;
                betsLost++;
            }
        }

        var stats = CalculateStats(bets.Count, betsWon, betsLost, moneyGained, moneyLost, matchesCanceled);

        string info =
            $"{user.userName}\n\nTotal Bets: {stats.totalBets}\nWin/Lose: {stats.winToLose}\nWin Percentage: {stats.winPercentage}\nGained Money: {stats.gainedMoney}\nLost Money: \n{stats.lostMoney}";

        InfoPanel.ShowPanel(Color.white, info);
    }

    private (string totalBets, string winToLose, string winPercentage, string gainedMoney, string lostMoney)
        CalculateStats(int betsCount, int betsWon, int betsLost,
            double moneyGained, double moneyLost, int matchesCanceled)
    {
        string totalBets = $"<color={ColorHelper.OrangeString}>{betsCount.ToString()}</color>";
        string winToLose;
        string winPercentage;
        if (betsCount > 0)
        {
            double percentage = (double)betsWon / (betsCount - matchesCanceled) * 100;

            switch (percentage)
            {
                case < 50:
                    winPercentage = $"<color={ColorHelper.HotPinkString}>{percentage.ToString("F2")}</color>% ";
                    winToLose =
                        $"<color={ColorHelper.HotPinkString}>{betsWon.ToString()} - {betsLost.ToString()}</color> ";
                    break;
                case > 50:
                    winPercentage = $"<color={ColorHelper.LightGreenString}>{percentage.ToString("F2")}</color>% ";
                    winToLose =
                        $"<color={ColorHelper.LightGreenString}>{betsWon.ToString()} - {betsLost.ToString()}</color> ";
                    break;
                default:
                    winPercentage = $"<color={ColorHelper.OrangeString}>{percentage.ToString("F2")}</color>% ";
                    winToLose =
                        $"<color={ColorHelper.OrangeString}>{betsWon.ToString()} - {betsLost.ToString()}</color> ";
                    break;
            }
        }
        else
        {
            winPercentage = "<color=#ffffff>N/A</color>% ";
            winToLose = $"<color=#ffffff>{betsWon.ToString()} - {betsLost.ToString()}</color>% ";
        }

        string gainedMoney =
            $"<color={ColorHelper.LightGreenString}>{moneyGained.ToString()}</color><color={ColorHelper.LightGreenString}>$</color>";
        string lostMoney =
            $"<color={ColorHelper.HotPinkString}>{moneyLost.ToString()}</color><color={ColorHelper.LightGreenString}>$</color>";

        return (totalBets, winToLose, winPercentage, gainedMoney, lostMoney);
    }
}