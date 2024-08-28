using System.Collections.Generic;
using System.Globalization;
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
                .Then(bets => BetsProcessor.ProcessBets(bets, matches, stats =>
                {
                    var betStats = CalculateStats(bets.Count, stats);

                    string info =
                        $"{user.userName}\n\nTotal Bets: {betStats.totalBets}\nWin/Lose: {betStats.winToLose}\nWin Percentage: \n{betStats.winPercentage}\nGained Money: \n{betStats.gainedMoney}\nLost Money: \n{betStats.lostMoney}";

                    InfoPanelManager.ShowPanel(Color.white, info);
                }))
                .Catch(exception =>
                {
                    InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                        $"<color={ColorHelper.WhiteString}>{user.userName}</color>\n\nUser has no bets or an error occurred.");
                    Debug.LogError(exception.Message);
                });
        }
    }


    private (string totalBets, string winToLose, string winPercentage, string gainedMoney, string lostMoney)
        CalculateStats(int betsCount, BetsProcessor.BetStats stats)
    {
        string totalBets = $"<color={ColorHelper.OrangeString}>{betsCount.ToString()}</color>";
        string winToLose;
        string winPercentage;
        if (betsCount > 0)
        {
            double percentage = (double)stats.BetsWon / (betsCount - stats.MatchesCanceled) * 100;

            switch (percentage)
            {
                case < 50:
                    winPercentage = $"<color={ColorHelper.HotPinkString}>{percentage.ToString("F2")}</color>% ";
                    winToLose =
                        $"<color={ColorHelper.HotPinkString}>{stats.BetsWon.ToString()} - {stats.BetsLost.ToString()}</color> ";
                    break;
                case > 50:
                    winPercentage = $"<color={ColorHelper.LightGreenString}>{percentage.ToString("F2")}</color>% ";
                    winToLose =
                        $"<color={ColorHelper.LightGreenString}>{stats.BetsWon.ToString()} - {stats.BetsLost.ToString()}</color> ";
                    break;
                default:
                    winPercentage = $"<color={ColorHelper.OrangeString}>{percentage.ToString("F2")}</color>% ";
                    winToLose =
                        $"<color={ColorHelper.OrangeString}>{stats.BetsWon.ToString()} - {stats.BetsLost.ToString()}</color> ";
                    break;
            }
        }
        else
        {
            winPercentage = $"<color={ColorHelper.WhiteString}>N/A</color>% ";
            winToLose = $"<color={ColorHelper.WhiteString}>{stats.BetsWon.ToString()} - {stats.BetsLost.ToString()}</color>% ";
        }

        string gainedMoney =
            $"<color={ColorHelper.LightGreenString}>{stats.MoneyGained.ToString()}</color><color={ColorHelper.LightGreenString}>$</color>";
        string lostMoney =
            $"<color={ColorHelper.HotPinkString}>{stats.MoneyLost.ToString()}</color><color={ColorHelper.LightGreenString}>$</color>";

        return (totalBets, winToLose, winPercentage, gainedMoney, lostMoney);
    }
}