using System;
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
        bool isWin,DateTime dateTime, bool isCanceled)
    {
        matchTitleTMP.text = matchTitle;
        contestantNameTMP.text = contestantName;
        coefficientTMP.text = coefficient.ToString("F2");
        betAmountTMP.text = $"{betAmount.ToString()}<color=#90EE90>$</color>";
        CheckWin(isWin, isActive, isCanceled,coefficient, betAmount);
        Date = dateTime;
    }

    private void CheckWin(bool isWin, bool isActive, bool isCanceled, double coefficient, double betAmount)
    {
        if (isActive)
        {
            SetStatus("Ongoing",  new Color32(0xFD, 0xFD, 0x96, 0xFF), betAmount);
            return;
        }

        if (isCanceled)
        {
            SetStatus("Canceled", new Color32(0xFD, 0xFD, 0x96,0xFF), betAmount);
            return;
        }

        if (isWin)
        {
            var winMoney = coefficient * betAmount;
            SetStatus("Win", new Color32(0x90, 0xEE, 0x90, 0xFF), winMoney);
            return;
        }

        SetStatus("Lose", new Color32(0xFF, 0x69, 0xB4,0xFF), -betAmount);
    }

    private void SetStatus(string status, Color32 color, double amount)
    {
        isWinTMP.text = status;
        isWinTMP.color = color;

        moneyLostOrGainedTMP.text = $"{amount.ToString()}<color=#90EE90>$</color>";
        moneyLostOrGainedTMP.color = color;
    }

}