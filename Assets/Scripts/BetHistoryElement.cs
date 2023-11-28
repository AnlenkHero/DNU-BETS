using System;
using TMPro;
using UnityEngine;

public class BetHistoryElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchTitleTMP;
    [SerializeField] private TextMeshProUGUI contestantNameTMP;
    [SerializeField] private TextMeshProUGUI coefficientTMP;
    [SerializeField] private TextMeshProUGUI betAmountTMP;
    [SerializeField] private TextMeshProUGUI isActiveTMP;
    [SerializeField] private TextMeshProUGUI isWinTMP;
    public DateTime Date;

    public void SetData(string matchTitle, string contestantName, double coefficient, double betAmount, bool isActive,
        bool isWin,DateTime dateTime, bool isCanceled)
    {
        matchTitleTMP.text = matchTitle;
        contestantNameTMP.text = contestantName;
        coefficientTMP.text = coefficient.ToString();
        betAmountTMP.text = $"{betAmount.ToString()}<color=#90EE90>$</color>";
        isActiveTMP.text = $"Active: {isActive.ToString()}";
        CheckWin(isWin, isActive, isCanceled);
        Date = dateTime;
    }

    private void CheckWin(bool isWin, bool isActive, bool isCanceled)
    {
        if (isActive)
        {
            isWinTMP.text = "Ongoing";
            isWinTMP.color = new Color32(0xFD, 0xFD, 0x96, 0xFF);
        }
        else
        {
            if (isCanceled)
            {
                isWinTMP.text = "Canceled";
                isWinTMP.color = new Color32(0xFD, 0xFD, 0x96, 0xFF);
            }
            else
            {
                if (isWin)
                {
                    isWinTMP.text = "Win";
                    isWinTMP.color = new Color32(0x90, 0xEE, 0x90, 0xFF);
                }
                else
                {
                    isWinTMP.text = "Lose";
                    isWinTMP.color = new Color32(0xFF, 0x69, 0xB4, 0xFF);
                }      
            }
        }
    }
}