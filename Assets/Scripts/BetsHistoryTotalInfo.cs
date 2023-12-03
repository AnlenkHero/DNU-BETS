using TMPro;
using UnityEngine;

public class BetsHistoryTotalInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalBets;
    [SerializeField] private TextMeshProUGUI winToLose;
    [SerializeField] private TextMeshProUGUI winPercentage;
    [SerializeField] private TextMeshProUGUI gainedMoney;
    [SerializeField] private TextMeshProUGUI lostMoney;

    public void SetData(int betsCount, int betsWon, int betsLost, double moneyGained, double moneyLost)
    {
        totalBets.text = betsCount.ToString();
        winToLose.text = $"{betsWon.ToString()} - {betsLost.ToString()}";

        if (betsCount > 0)
        {
            double percentage = (double)betsWon / betsCount * 100;
            winPercentage.text = $"{percentage.ToString("F2")}<color=#000000>%</color> ";
            
            switch (percentage)
            {
                case < 50:
                    winPercentage.color = new Color32(0xFF, 0x69, 0xB4, 0xFF); 
                    winToLose.color = new Color32(0xFF, 0x69, 0xB4, 0xFF);
                    break;
                case > 50:
                    winPercentage.color = new Color32(0x90, 0xEE, 0x90, 0xFF); 
                    winToLose.color = new Color32(0x90, 0xEE, 0x90, 0xFF);
                    break;
                default:
                    winPercentage.color = new Color32(0xFD, 0xFD, 0x96, 0xFF);
                    winToLose.color = new Color32(0xFD, 0xFD, 0x96, 0xFF);
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
        lostMoney.text = $"{moneyLost.ToString()}<color=#90EE90>$</color>";
    }
}

