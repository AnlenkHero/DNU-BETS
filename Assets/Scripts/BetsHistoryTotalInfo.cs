using Libs.Helpers;
using TMPro;
using UnityEngine;

public class BetsHistoryTotalInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalBets;
    [SerializeField] private TextMeshProUGUI winToLose;
    [SerializeField] private TextMeshProUGUI winPercentage;
    [SerializeField] private TextMeshProUGUI gainedMoney;
    [SerializeField] private TextMeshProUGUI lostMoney;

    public void SetData(int betsCount, int betsWon, int betsLost, double moneyGained, double moneyLost, int matchesCanceled)
    {
        totalBets.text = betsCount.ToString();
        winToLose.text = $"{betsWon.ToString()} - {betsLost.ToString()}";

        if (betsCount > 0 && (betsCount - matchesCanceled) > 0)
        {
            double percentage = (double)betsWon / (betsCount - matchesCanceled) * 100;
            winPercentage.text = $"{percentage:F2}<color=#000000>%</color> ";
            
            switch (percentage)
            {
                case < 50:
                    winPercentage.color = ColorHelper.HotPink; 
                    winToLose.color = ColorHelper.HotPink;
                    break;
                case > 50:
                    winPercentage.color = ColorHelper.LightGreen; 
                    winToLose.color = ColorHelper.LightGreen;
                    break;
                default:
                    winPercentage.color = ColorHelper.Orange;
                    winToLose.color = ColorHelper.Orange;
                    break;
            }
        }
        else
        {
            winPercentage.text = "N/A";
            winPercentage.color = Color.black; 
            winToLose.color = Color.black; 
        }

        gainedMoney.text = $"{moneyGained.ToString()}$";
        lostMoney.text = $"{moneyLost.ToString()}<color={ColorHelper.LightGreenString}>$</color>";
    }
}

