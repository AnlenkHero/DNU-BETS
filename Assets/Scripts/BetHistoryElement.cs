using System;
using Libs.Helpers;
using TMPro;
using UnityEngine;

public class BetHistoryElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchTitleTMP;
    [SerializeField] private TextMeshProUGUI contestantNameTMP;
    [SerializeField] private TextMeshProUGUI coefficientTMP;
    [SerializeField] private TextMeshProUGUI betAmountTMP;
    [SerializeField] private TextMeshProUGUI moneyLostOrGainedTMP;
    [SerializeField] private TextMeshProUGUI isWinTMP;
    public DateTime Date;

    public void SetData(string matchTitle, string contestantName, double coefficient, double betAmount, bool isActive,
        bool isWin, DateTime dateTime, bool isCanceled)
    {
        matchTitleTMP.text = matchTitle;
        contestantNameTMP.text = contestantName;
        coefficientTMP.text = coefficient.ToString("F2");
        betAmountTMP.text = $"{betAmount.ToString()}<color={ColorHelper.LightGreenString}>$</color>";
        CheckWin(isWin, isActive, isCanceled, coefficient, betAmount);
        Date = dateTime;
    }

    private void CheckWin(bool isWin, bool isActive, bool isCanceled, double coefficient, double betAmount)
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
}