using System;
using Libs.Helpers;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetHistoryElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchTitleTMP;
    [SerializeField] private TextMeshProUGUI contestantNameTMP;
    [SerializeField] private TextMeshProUGUI coefficientTMP;
    [SerializeField] private TextMeshProUGUI betAmountTMP;
    [SerializeField] private TextMeshProUGUI moneyLostOrGainedTMP;
    [SerializeField] private TextMeshProUGUI isWinTMP;
    [SerializeField] private Button betHistoryButton;
    public DateTime Date;

    public void SetData(BetsProcessor.ProcessedBetDetail processedBetDetail)
    {
        var contestant = processedBetDetail.Match.Contestants.Find(c => c.Id == processedBetDetail.Bet.ContestantId);

        matchTitleTMP.text = processedBetDetail.Match.MatchTitle;
        contestantNameTMP.text = contestant.Name;
        coefficientTMP.text = contestant.Coefficient.ToString("F2");
        
        betAmountTMP.text =
            $"{processedBetDetail.Bet.BetAmount.ToString()}<color={ColorHelper.LightGreenString}>$</color>";
        
        Date = processedBetDetail.DateTime;
        
        CheckWin(processedBetDetail.Bet.IsActive, processedBetDetail.IsWinner, processedBetDetail.Match.IsMatchCanceled,
            contestant.Coefficient, processedBetDetail.Bet.BetAmount);
        
        betHistoryButton.onClick.AddListener(() => ShowAdditionalInfo(processedBetDetail.Match, contestant));
    }

    private void CheckWin(bool isActive, bool isWin, bool isCanceled, double coefficient, double betAmount)
    {
        if (isActive)
        {
            SetStatus("Ongoing", ColorHelper.PaleYellow, betAmount);
            return;
        }

        if (isCanceled)
        {
            SetStatus("Canceled", ColorHelper.PaleYellow, betAmount);
            return;
        }

        if (isWin)
        {
            var winMoney = coefficient * betAmount;
            SetStatus("Win", ColorHelper.LightGreen, winMoney);
            return;
        }

        SetStatus("Lose", ColorHelper.HotPink, -betAmount);
    }

    private void SetStatus(string status, Color32 color, double amount)
    {
        isWinTMP.text = status;
        isWinTMP.color = color;

        moneyLostOrGainedTMP.text = $"{amount.ToString()}<color={ColorHelper.LightGreenString}>$</color>";
        moneyLostOrGainedTMP.color = color;
    }

    private void ShowAdditionalInfo(Match match, Contestant contestant)
    {
        InfoPanelManager.ShowPanel(Color.white,
            $"{match.MatchTitle}\n\n{GetAllContestantsInfo(match, contestant)}\n\n{Date.ToLocalTime():f}\n", () =>
            {
                LoadImageAndDisplayPanel(match);
                InfoPanelManager.Instance.AddButton("Close", InfoPanelManager.Instance.HidePanel);
            });
    }

    private string GetAllContestantsInfo(Match match, Contestant contestantBet)
    {
        string info = "";

        foreach (var contestant in match.Contestants)
        {
            if (contestant.Winner)
            {
                info += $"<color={ColorHelper.LightGreenString}>{contestant.Name} - {contestant.Coefficient}</color>\n";
                continue;
            }

            if (contestant == contestantBet)
            {
                info += $"<color={ColorHelper.PaleYellowString}>{contestant.Name} - {contestant.Coefficient}</color>\n";
                continue;
            }

            info += $"<color={ColorHelper.HotPinkString}>{contestant.Name} - {contestant.Coefficient}</color>\n";
        }

        return info;
    }

    private void LoadImageAndDisplayPanel(Match match)
    {
        InfoPanelManager.Instance.SetImage(null);
        TextureLoader.LoadTexture(this, match.ImageUrl, texture2D =>
        {
            if (texture2D != null)
                InfoPanelManager.Instance.SetImage(texture2D);
            else
                Debug.Log("Texture failed to load.");
        });
    }
}